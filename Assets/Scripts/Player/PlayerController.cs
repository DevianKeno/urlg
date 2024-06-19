using System;
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
        [SerializeField] Image vignette;

        [SerializeField] PlayerStateMachine stateMachine;
        public PlayerStateMachine sm => stateMachine;
        [SerializeField] PlayerAnimator animator;
        [SerializeField] PlayerStatsManager stats;
        public PlayerStatsManager Stats => stats;

        Rigidbody2D rb;
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
            rb = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInput>();
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
            var go = collider.gameObject;
            
            if (go.CompareTag("Enemy"))
            {
                TakeDamage(1);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isInvincible) return;
            _isInvincible = true;

            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, 0.5f, 0, 1f)
                .setOnUpdate((float i) =>
                {
                    vignette.color = new(0.5f, 0f, 0f, i);
                })
                .setEase(LeanTweenType.easeOutSine)
                .setOnComplete(() =>
                {
                    _isInvincible = false;
                });
            Game.Telemetry.PlayerStats["hitsTaken"].Increment();
        }

        void FixedUpdate()
        {
            Vector2 targetVelocity = _frameMovement * MoveSpeed;
            _currentVelocity = Vector2.Lerp(_currentVelocity, targetVelocity, Acceleration * Time.fixedDeltaTime);
            rb.velocity = _currentVelocity;
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
            sm.ToState(PlayerStates.Shoot); /// This should be locked
            
            _fireRateDelta = 0f;
            Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mPos.z = 0f;

            var direction = (mPos - transform.position).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            var go = Instantiate(Equipped.ProjectileData.Object, transform.position, rotation);
            if (go.TryGetComponent(out Projectile proj))
            {
                proj.Owner = this;
                proj.SetDirection(direction);
            }            
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                rb.velocity = Vector2.zero;
                
                // if 
            }
        }
    }
}
