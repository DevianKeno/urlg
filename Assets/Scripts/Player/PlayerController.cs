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

namespace RL.Player
{
    public class PlayerController : MonoBehaviour
    {
        public const float InvincibilityFrameSeconds = 0.5f;

        public float Health = 100f;
        public float MoveSpeed = 7f;
        public float Acceleration = 0.3f;


        bool _isHoldingFire;
        bool _isInvincible;
        float _fireRateDelta;
        Vector2 _frameMovement;
        Vector2 _currentVelocity;

        [Header("Components")]
        [SerializeField] PlayerStateMachine stateMachine;
        public PlayerStateMachine StateMachine => stateMachine;
        [SerializeField] PlayerAnimator animator;
        // [SerializeField] PlayerStatsManager stats;
        // public PlayerStatsManager Stats => stats;

        [SerializeField] Rigidbody2D rb;
        public Rigidbody2D Rigidbody2D => rb;
        [SerializeField] SpriteRenderer spriteRenderer;

        PlayerInput input;
        Dictionary<string, InputAction> inputs = new();

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

            Weapon1 = Fireball;
            Weapon2 = Laser;
            unequippedWeapon = Wave;

            StateMachine.OnStateChanged += animator.StateChangedCallback;
            StateMachine.ToState(PlayerStates.Idle);
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
            }
            else
            {
                inputs["Move"].Disable();
                inputs["Shoot"].Disable();
                inputs["Swap Weapons"].Disable();
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
            var other = collider.gameObject;
            
            if (other.CompareTag("Enemy"))
            {
                TakeDamage(1);
                return;
            }
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
            if (_isInvincible) return;
            _isInvincible = true;

            Health -= damage;
            Game.Telemetry.PlayerStats[StatKey.HitsTaken].Increment();
            Game.UI.VignetteDamageFlash();
            
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
                Die();
                return true;
            }
            return false;
        }

        void Die()
        {
            
        }

        IEnumerator InvincibilityFrameCoroutine()
        {
            _isInvincible = true;
            yield return new WaitForSeconds(InvincibilityFrameSeconds);
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
                    Equipped = Weapon1;
                }
                else if (index == 2)
                {
                    Equipped = Weapon2;
                }
            }
        }

        void OnInputCheats(InputAction.CallbackContext context)
        {            
            if (!int.TryParse(context.control.displayName, out int index)) return;

            Equipped = index switch
            {
                1 => Fireball,
                2 => Laser,
                3 => Wave,
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
        }
    }
}
