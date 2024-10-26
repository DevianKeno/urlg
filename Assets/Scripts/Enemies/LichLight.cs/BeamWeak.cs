using System;
using System.Collections;
using UnityEngine;
using RL.Systems;
using RL.Player;
using RL.Entities;
using Random = UnityEngine.Random;
using RL.Telemetry;

namespace RL.Enemies
{
    /// LichLight
    public class BeamWeak : Enemy, IDamageable
    {
        [SerializeField] GameObject barrier;

        [Header("Enemy Parameters")]
        public float ContactDamage = 10f;

        [Header("Detection Parameters")]
        public float detectionRadius = 5f;
        public float detectionAngle = 45f;
        public LayerMask detectionMask;
        public float lungeForce = 5f;
        public float lungeDistance = 2f;
        public float lungeCooldown = 1f;
        bool _canLunge = true;
        bool _isLunging;

        [Header("Charge Parameters")]
        public float chargeSpeed = 10f; // Speed of the charge
        public float overshoot = 2f; // Force applied to overshoot the player
        public float chargeCooldown = 5f; // Cooldown between charge attacks
        public float minChargeInterval = 2f; // Minimum time between charge attacks
        public float maxChargeInterval = 2f; // Maximum time between charge attacks
        public float chargeWindup = 1f; // Duration of the windup before charging
        public float chargeDuration = 2f; // Force applied to overshoot the player
        float chargeInterval;
        float chargeDelta;
        [SerializeField] bool _canCharge = true;
        [SerializeField] bool _isCharging = false;

        [SerializeField] LichLightStateMachine stateMachine;
        public StateMachine<LichLightStates> sm => stateMachine;
        [SerializeField] LichLightAnimator animator;

        protected override void Start()
        {
            base.Start();
            chargeInterval = UnityEngine.Random.Range(minChargeInterval, maxChargeInterval);
            chargeDelta = 0f;
            barrier.SetActive(false);
            sm.OnStateChanged += animator.StateChangedCallback;

            sm.ToState(LichLightStates.Idle);
        }

        protected override void FixedUpdate()
        {
            if (IsAsleep) return;
            
            Search();
            LookAtTarget();            
            if (!_isLunging && !_isCharging)
            {
                MaintainDistance();
            }
            if (!_isCharging)
            {
                DetectProjectiles();
            }
            UpdateStates();
        }

        void UpdateStates()
        {
            if (target != null)
            {
                if (!_isCharging)
                {
                    sm.ToState(LichLightStates.Move);
                }
            } else
            {
                sm.ToState(LichLightStates.Idle);
            }
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            // Debug.Log("trigger collision");
            var go = collider.gameObject;
            
            if (go == null) return;
            if (go.CompareTag("Player"))
            {
                if (go.TryGetComponent(out PlayerController player))
                {
                    // Debug.Log("player hit");
                    player.TakeDamage(ContactDamage);
                }
            }
        }

        void Update()
        {
            chargeDelta += Time.deltaTime;
            if (chargeDelta >= chargeInterval)
            {
                if (target == null)
                {
                    chargeInterval = maxChargeInterval;
                    chargeDelta = 0f;
                } else
                {
                    if (_canCharge && !_isCharging)
                    {
                        _canCharge = false;
                        StartCoroutine(ChargeCoroutine());
                    }
                }
            }
        }

        void LateUpdate()
        {
            spriteRenderer.transform.rotation = Quaternion.identity;
            FlipSprite();
        }

        void FlipSprite()
        {
            if (rb.rotation > 90 || rb.rotation < -90)
            {
                spriteRenderer.flipX = false;
            } else
            {
                spriteRenderer.flipX = true;
            }
        }

        float EaseOutCubic(float x)
        {
            return 1 - Mathf.Pow(1 - x, 3);
        }

        IEnumerator ChargeCoroutine()
        {
            if (target == null) yield return null;

            _isCharging = true;
            sm.ToState(LichLightStates.Move);

            /// Windup
            rb.velocity = Vector2.zero;
            barrier.SetActive(true);
            yield return new WaitForSeconds(chargeWindup);

            Vector2 targetPosition = target.transform.position;
            Vector2 overshootDirection = (targetPosition - (Vector2) transform.position).normalized;
            Vector2 overshootPosition = targetPosition + (overshootDirection * overshoot);
            float startTime = Time.time;
            float journeyLength = Vector2.Distance(transform.position, overshootPosition);

            Game.Telemetry.RoomStats[StatKey.EnemyAttackCount].Increment();
            sm.ToState(LichLightStates.Move);
            while (Time.time - startTime < chargeDuration)
            {
                float distanceCovered = (Time.time - startTime) * chargeSpeed;
                rb.MovePosition(Vector2.Lerp(transform.position, overshootPosition, EaseOutCubic(distanceCovered / journeyLength)));
                yield return null;
            }
            
            barrier.SetActive(false);
            _isCharging = false;
            yield return new WaitForSeconds(chargeCooldown);
            chargeInterval = UnityEngine.Random.Range(minChargeInterval, maxChargeInterval);
            chargeDelta = 0f;
            _canCharge = true;
        }


        void DetectProjectiles()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 directionToCollider = collider.transform.position - transform.position;
                float angle = Vector2.Angle(transform.up, directionToCollider);

                if (angle <= detectionAngle)
                {
                    if (_canLunge)
                    {
                        Lunge();
                        StartCoroutine(Cooldown());
                    }
                }
            }
        }


        IEnumerator Cooldown()
        {
            _canLunge = false;
            yield return new WaitForSeconds(lungeCooldown);
            _isLunging = false;
            _canLunge = true;
        }

        /// <summary>
        /// -1 (left)
        /// 0 (random)
        /// 1 (right)
        /// </summary>
        void Lunge(int direction = 0)
        {
            _isLunging = true;
            direction = direction != 0 ? direction : UnityEngine.Random.Range(0, 2) * 2 - 1;

            Vector2 lungeDirection = transform.right * direction;
                
            RaycastHit2D hit = Physics2D.Raycast(transform.position, lungeDirection, lungeDistance);
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
    {
                direction *= -1;
                lungeDirection = transform.right * direction;
            }
            rb.AddForce(lungeDirection * lungeForce, ForceMode2D.Impulse);
            sm.ToState(LichLightStates.Move);
            sm.LockFor(0.5f);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            // Gizmos.color = Color.yellow;
            Vector3 rightDirection = Quaternion.Euler(0, 0, -detectionAngle) * transform.right;
            Gizmos.DrawRay(transform.position, rightDirection * detectionRadius);
            Vector3 leftDirection = Quaternion.Euler(0, 0, detectionAngle) * transform.right;
            Gizmos.DrawRay(transform.position, leftDirection * detectionRadius);

            Gizmos.color = Color.red;
            float startAngle = -detectionAngle;
            float endAngle = detectionAngle;
            int segments = 30;
            float step = (endAngle - startAngle) / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = startAngle + step * i;
                Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * transform.up;
                Gizmos.DrawRay(transform.position, rayDirection * detectionRadius);
            }
        }
    }
}
