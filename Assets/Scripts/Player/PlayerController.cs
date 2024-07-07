using System;
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using RL.Weapons;
using RL.Projectiles;
using RL.Systems;

namespace RL.Player
{
    public class PlayerController : MonoBehaviour
    {
        public float Health = 100f;
        public float MoveSpeed = 7f;
        public float Acceleration = 0.3f;
        public Weapon Equipped;
        public Weapon Primary;
        public Weapon Secondary;
        public Weapon Tertiary;

        bool _isInvincible;
        Vector2 _frameMovement;
        Vector2 _currentVelocity;
        float _fireRateDelta;

        [Header("Components")]
        [SerializeField] PlayerStateMachine stateMachine;
        public PlayerStateMachine sm => stateMachine;
        [SerializeField] PlayerAnimator animator;
        [SerializeField] PlayerStatsManager stats;
        public PlayerStatsManager Stats => stats;
        [SerializeField] Rigidbody2D rb;
        [SerializeField] SpriteRenderer spriteRenderer;

        PlayerInput input;
        InputAction move;
        InputAction shoot;
        InputAction swap;
        InputAction cheats;

        [Space(10)]
        public bool EnableCheats;
        [SerializeField] Weapon Fireball;
        [SerializeField] Weapon Laser;
        [SerializeField] Weapon Wave;


        void Awake()
        {
            // rb = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInput>();
            // spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            // tileBackLayer = SortingLayer.NameToID("Tiles Back");
            // tileFrontLayer = SortingLayer.NameToID("Tiles Front");
        }

        void Start()
        {
            var map = input.actions.FindActionMap("Player");
            move = map.FindAction("Move");
            shoot = map.FindAction("Shoot");
            swap = map.FindAction("Swap Weapons");
            cheats = map.FindAction("Cheats");

            move.performed += (c) =>
            {
                _frameMovement = c.ReadValue<Vector2>();
                sm.ToState(PlayerStates.Move);
            };
            move.canceled += (c) =>
            {
                _frameMovement = Vector2.zero;
                sm.ToState(PlayerStates.Idle);
            };
            shoot.performed += ShootCallback;
            swap.performed += SwapCallback;

            shoot.Enable();
            move.Enable();
            if (EnableCheats)
            {
                cheats.performed += CheatsCallback;
                cheats.Enable();
            }
            sm.OnStateChanged += animator.StateChangedCallback;
            sm.ToState(PlayerStates.Idle);
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

        void Update()
        {
            _fireRateDelta += Time.deltaTime;
            if (shoot.IsPressed())
            {
                Shoot();
            }
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

        public void TakeDamage(float damage)
        {
            if (_isInvincible) return;
            _isInvincible = true;

            Health -= damage;
            Game.Telemetry.PlayerStats["hitsTaken"].Increment();
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
            yield return new WaitForSeconds(0.5f);
            _isInvincible = false;
        }

        void FixedUpdate()
        {
            Vector2 targetVelocity = _frameMovement * MoveSpeed;
            _currentVelocity = Vector2.Lerp(_currentVelocity, targetVelocity, Acceleration * Time.fixedDeltaTime);
            rb.velocity = _currentVelocity;
            FlipSprite();
        }

        void CheatsCallback(InputAction.CallbackContext context)
        {            
            if (!int.TryParse(context.control.displayName, out int index)) return;
            Equipped = index switch
            {
                1 => Fireball,
                2 => Laser,
                3 => Wave,
                _ => throw new NotImplementedException(),
            };
        }

        void SwapCallback(InputAction.CallbackContext c)
        {
            if (c.ReadValue<float>() != 0)
            {
                Equipped = Equipped == Primary ? Secondary : Primary;
            }
        }

        void ShootCallback(InputAction.CallbackContext c)
        {
            if (Equipped == null) return;
            Shoot();
        }

        public void SaveStats()
        {
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "statistics");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var path = Path.Combine(Application.persistentDataPath, "statistics", $"test.json");
                string json = JsonUtility.ToJson(stats.Stats);
                File.WriteAllText(path, json);
                Debug.Log($"PlayerStats saved to {path}");
            } catch
            {
            }
        }

        public void Shoot()
        {
            if (_fireRateDelta < Equipped.FireRate) return;
            sm.ToState(PlayerStates.Shoot); // This should be locked
            
            _fireRateDelta = 0f;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector3 spawnOffset = (mousePos - transform.position).normalized * 0.5f; // Adjust the forward distance here
            Vector3 spawnPosition = transform.position + spawnOffset;

            Vector3 direction = (mousePos - spawnPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject projectileGO = Instantiate(Equipped.ProjectileData.Object, spawnPosition, rotation);
            if (projectileGO.TryGetComponent(out Projectile projectile))
            {
                projectile.Owner = this;
                projectile.SetDirection(direction);
            }
        }
    }
}
