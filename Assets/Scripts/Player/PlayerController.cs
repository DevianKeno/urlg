using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using RL.Weapons;
using RL.Projectiles;
using RL.Systems;
using RL.Telemetry;
using RL.UI;
using RL.Entities;
using UnityEngine.SceneManagement;

namespace RL.Player
{
    public class PlayerController : MonoBehaviour
    {
        public const float InvincibilityTime = 2f;
        public HealthBar healthBar;
        public WaveWeak salaman;
        public float MaximumHealth = 100f;
        public float Health;
        public float MoveSpeed = 7f;
        public float Acceleration = 0.3f;

        public bool IsAlive { get; private set; }
        bool _isHoldingFire;
        bool _isInvincible;
        bool _pauseScreenIsVisible;
        bool _enablePauseControl = false;
        float _fireRateDelta;
        Vector2 _frameMovement;
        Vector2 _currentVelocity;

        bool _invincibilityFrameActive;
        float _invincibilityFramesTimer;
        MainMenuWindow mainMenuWindow;
        [SerializeField] Canvas levelCanvas;

        [Header("Components")]
        [SerializeField] PlayerStateMachine stateMachine;
        public PlayerStateMachine StateMachine => stateMachine;
        [SerializeField] PlayerAnimator animator;
        [SerializeField] Rigidbody2D rb;
        public Rigidbody2D Rigidbody2D => rb;

        [SerializeField] SpriteRenderer spriteRenderer;
        DamageVignette damageVignette;
        WeaponsDisplayUI weaponsDisplayUI;

        PlayerInput input;
        Dictionary<string, InputAction> inputs = new();

        int selectedWeapon = 0;
        public Weapon Equipped;
        public Weapon Weapon1;
        public Weapon Weapon2;
        Weapon unequippedWeapon;
        
        [Space(10)]
        public bool EnableCheats;
        [SerializeField] Weapon Fireball;
        [SerializeField] Weapon Laser;
        [SerializeField] Weapon Wave;


        #region Initializing methods

        void Awake()
        {
            input = GetComponent<PlayerInput>();
        }

        void Start()
        {
            InitializeInputs();
            InitializeWeapons();

            if (damageVignette == null)
            {
                damageVignette = FindObjectOfType<DamageVignette>();
            }
            if (healthBar == null)
            {
                healthBar = FindObjectOfType<HealthBar>();
            }
            damageVignette = Game.UI.Create<DamageVignette>("Damage Vignette", parent: levelCanvas.transform);
            weaponsDisplayUI = Game.UI.Create<WeaponsDisplayUI>("Weapons Display UI", parent: levelCanvas.transform);
            weaponsDisplayUI.EnableSwapping = true;
            UpdateWeapons();
            
            StateMachine.OnStateChanged += animator.StateChangedCallback;
            StateMachine.ToState(PlayerStates.Idle);
            healthBar.InitializeMaxHealth(MaximumHealth);
            _isInvincible = false;
            IsAlive = true;
            StartCoroutine(PauseControlTimerCoroutine());
        }

        void InitializeWeapons()
        {
            if (Game.Main.currentLevel <= 1)
            {
                Game.Main.PlayerEquippedWeapon1 = Fireball;
                Game.Main.PlayerEquippedWeapon2 = Laser;
                Game.Main.PlayerUnequippedWeapon = Wave;
            }
            
            Weapon1 = Game.Main.PlayerEquippedWeapon1;
            Weapon2 = Game.Main.PlayerEquippedWeapon2;
            unequippedWeapon = Game.Main.PlayerUnequippedWeapon;
                        
            RefreshEquipped();
        }

        void RefreshEquipped()
        {                        
            if (selectedWeapon <= 1)
            {
                Equipped = Weapon1;
            }
            else ///(selectedWeapon == 2)
            {
                Equipped = Weapon2;
            }
        }

        void InitializeInputs()
        {
            var map = input.actions.FindActionMap("Player");
            string[] getInputs = new[]
            {
                "Move", "Shoot", "Swap Weapons", "Cheats",
            };

            foreach (var str in getInputs)
            {
                var inputAction = map.FindAction(str);
                if (inputAction != null)
                {
                    inputs[str] = inputAction;
                }
            }

            inputs["Move"].performed += OnInputMove;
            inputs["Move"].canceled += OnInputMove;

            inputs["Shoot"].started += OnInputShoot;
            inputs["Shoot"].canceled += OnInputShoot;

            inputs["Swap Weapons"].performed += OnInputSwap;
            inputs["Cheats"].started += OnInputCheats;

            inputs["Shoot"].Enable();
            inputs["Swap Weapons"].Enable();

            if (EnableCheats)
            {
                inputs["Cheats"].Enable();
            }
            else
            {
                inputs["Cheats"].Disable();
            }
        }

        #endregion

        public void SetControlsEnabled(bool enable)
        {
            if (enable)
            {
                inputs["Move"].Enable();
                inputs["Shoot"].Enable();
                inputs["Swap Weapons"].Enable();
                weaponsDisplayUI.EnableSwapping = true;
                _enablePauseControl = true;
            }
            else
            {
                inputs["Move"].Disable();
                inputs["Shoot"].Disable();
                inputs["Swap Weapons"].Disable();
                weaponsDisplayUI.EnableSwapping = false;
                _enablePauseControl = false;
            }
        }

        public void SetEquippedWeapon1(Weapon weapon)
        {
            this.Weapon1 = weapon;
        }
        
        public void SetEquippedWeapon2(Weapon weapon)
        {
            this.Weapon2 = weapon;
        }

        void Update()
        {
            _fireRateDelta += Time.deltaTime;

            if (_isHoldingFire)
            {
                Shoot();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pauseScreenIsVisible)
                {
                    HidePauseMenu();
                }
                else if (!_pauseScreenIsVisible && _enablePauseControl)
                {
                    ShowPauseMenu();
                }
            }

            if (_isInvincible)
            {
                _invincibilityFramesTimer += Time.deltaTime;

                if (_invincibilityFramesTimer > 0.3f && !_invincibilityFrameActive)
                {
                    _invincibilityFramesTimer = 0;
                    _invincibilityFrameActive = true;

                    spriteRenderer.color = new Color(
                        spriteRenderer.color.r,
                        spriteRenderer.color.g,
                        spriteRenderer.color.b,
                        0.5f);

                    LeanTween.value(gameObject, 0.5f, 1f, 0.1f)
                        .setLoopPingPong(1)
                        .setOnUpdate((float alpha) =>
                        {
                            var color = spriteRenderer.color;
                            color.a = alpha;
                            spriteRenderer.color = color;
                        })
                        .setOnComplete(() => _invincibilityFrameActive = false);
                }
            }
            else
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    1f);
            }

            /// Test damage
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                TakeDamage(20);
            }
        }

        void ShowPauseMenu()
        {
            _pauseScreenIsVisible = true;
            
            if (mainMenuWindow == null)
            {
                SetControlsEnabled(false);
                mainMenuWindow = Game.UI.Create<MainMenuWindow>("Main Menu Window");
                mainMenuWindow.OnClose += () =>
                {
                    _pauseScreenIsVisible = false;
                    SetControlsEnabled(true);
                };
            }
        }

        void HidePauseMenu()
        {
            _pauseScreenIsVisible = false;
            SetControlsEnabled(true);
            mainMenuWindow?.Hide(destroy: true);
        }

        void FixedUpdate()
        {
            Vector2 targetVelocity = _frameMovement * MoveSpeed;
            _currentVelocity = Vector2.Lerp(_currentVelocity, targetVelocity, Acceleration * Time.fixedDeltaTime);
            rb.velocity = _currentVelocity;
            FlipSprite();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            // var other = collider.gameObject;
            
            // if (other.CompareTag("Enemy"))
            // {
            //     if (other.TryGetComponent(out Enemy enemy))
            //     {
            //         TakeDamage(enemy.ContactDamage);
            //     }
            //     return;
            // }
        }

        void FlipSprite()
        {
            if (rb.velocity.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (rb.velocity.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }

        public void SwapEquipped(int index)
        {
            index = System.Math.Clamp(index, 0, 1);

            /// Swap equipped weapons with unequipped one
            if (index == 0)
            {
                (unequippedWeapon, Weapon1) = (Weapon1, unequippedWeapon);
            }
            else if (index == 1)
            {
                (unequippedWeapon, Weapon2) = (Weapon2, unequippedWeapon);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isInvincible || !IsAlive) return;

            _invincibilityFramesTimer = 0.3f;
            Health -= damage;
            healthBar.UpdateHealthPoints(Health);
            Game.Telemetry.PlayerStats[StatKey.HitsTaken].Increment();
            damageVignette.DamageFlash();

            Game.Audio.Play("damaged");
            
            if (CheckIfDead())
            {
                return;
            }

            StartCoroutine(nameof(InvincibilityFrameCoroutine));
        }

        bool CheckIfDead()
        {
            if (Health <= 0)
            {
                StartCoroutine(DieCoroutine());
                return true;
            }
            return false;
        }

        IEnumerator DieCoroutine()
        {
            if (!IsAlive) yield break;
            
            IsAlive = false;
            SetControlsEnabled(false);
            StateMachine.ToState(PlayerStates.Death);
            var puffParticle = Game.Particles.Create("puff");
            puffParticle.transform.position = transform.position;
            Game.Main.CurrentRoom?.SleepAllEnemies();

            Game.Audio.StopMusic("level");
            Game.Audio.Play("meow");

            spriteRenderer.enabled = false;
            Game.Telemetry.IncrementDeathCount();

            yield return new WaitForSeconds(2);

            ReloadLevel();
        }

        void ReloadLevel()
        {
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LOADING",
                    Mode = LoadSceneMode.Additive,
                    PlayTransition = true, },
                onLoadSceneCompleted: () =>
                {
                    Game.Main.UnloadScene(
                        "LEVEL",
                        onUnloadSceneCompleted: () =>
                        {
                            Game.Main.LoadScene(
                                new(){
                                    SceneToLoad = "LEVEL",
                                });
                        });
                });
        }

        public void Heal()
        {
            healthBar.UpdateHealthPoints(MaximumHealth);
            
            var healths = Resources.Load<GameObject>("Prefabs/Healths");
            Instantiate(healths, transform);
        }

        public void UpdateWeapons()
        {
            weaponsDisplayUI ??= GameObject.FindAnyObjectByType<WeaponsDisplayUI>();
            if (weaponsDisplayUI == null) return;

            weaponsDisplayUI.weapon1.ProjectileData = Weapon1.ProjectileData;
            weaponsDisplayUI.weapon2.ProjectileData = Weapon2.ProjectileData;
            
            RefreshEquipped();
        }
        
        public void SaveWeapons()
        {
            Game.Main.PlayerEquippedWeapon1 = Weapon1;
            Game.Main.PlayerEquippedWeapon2 = Weapon2;
            Game.Main.PlayerUnequippedWeapon = unequippedWeapon;
        }

        IEnumerator InvincibilityFrameCoroutine()
        {
            _isInvincible = true;
            yield return new WaitForSeconds(InvincibilityTime);
            _isInvincible = false;
        }


        #region Player input callbacks
        
        void OnInputMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _frameMovement = context.ReadValue<Vector2>();
                StateMachine.ToState(PlayerStates.Move);
            }
            else if (context.canceled)
            {
                _frameMovement = Vector2.zero;
                StateMachine.ToState(PlayerStates.Idle);
            }
        }

        void OnInputShoot(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _isHoldingFire = true;
            }
            else if (context.canceled)
            {
                _isHoldingFire = false;
            }
        }

        void OnInputSwap(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!int.TryParse(context.control.displayName, out int index)) return;

                index = System.Math.Clamp(index, 1, 2);
                if (index == 1)
                {
                    selectedWeapon = 1;
                    Equipped = Weapon1;
                }
                else if (index == 2)
                {
                    selectedWeapon = 2;
                    Equipped = Weapon2;
                }
            }
        }

        void OnInputCheats(InputAction.CallbackContext context)
        {            
            if (!char.TryParse(context.control.displayName, out char key)) return;

            Equipped = key switch
            {
                'Z' => Fireball,
                'X' => Laser,
                'C' => Wave,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        #endregion


        #region Public methods

        // public void SaveStats()
        // {
        //     try
        //     {
        //         string directory = Path.Combine(Application.persistentDataPath, "saves");
        //         if (!Directory.Exists(directory))
        //         {
        //             Directory.CreateDirectory(directory);
        //         }

        //         var path = Path.Combine(Application.persistentDataPath, "saves", $"playerdata.json");
        //         string json = JsonUtility.ToJson(stats.Stats);
        //         File.WriteAllText(path, json);
        //         Debug.Log($"PlayerStats saved to {path}");
        //     } catch
        //     {
        //     }
        // }

        #endregion


        void Shoot()
        {          
            if (_fireRateDelta < Equipped.FireRate) return;

            StateMachine.ToState(PlayerStates.Shoot); /// This should be locked
            
            _fireRateDelta = 0f;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector3 spawnOffset = (mousePos - transform.position).normalized * 0.5f; /// Adjust the forward distance here
            Vector3 spawnPosition = transform.position + spawnOffset;

            Vector3 direction = (mousePos - spawnPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject projectileGO = Instantiate(Equipped.ProjectileData.Object, spawnPosition, rotation);
            if (projectileGO.TryGetComponent(out Projectile projectile))
            {
                projectile.SetOwner(this);
                projectile.SetDirection(direction);
            }

            var wakeId = Equipped.ProjectileData.Type switch
            {
                WeaponType.Fireball => "wake_fireball",
                WeaponType.Beam => "wake_beam",
                WeaponType.Wave => "wake_wave",
            };

            Game.Particles.Create(wakeId).transform.position = transform.position;
        }

        IEnumerator PauseControlTimerCoroutine()
        {
            yield return new WaitForSeconds(2);
            _enablePauseControl = true;
        }
    }
}
