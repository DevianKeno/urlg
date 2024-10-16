using System;
using System.Collections;

using UnityEngine;

using RL.Systems;
using RL.Player;
using RL.Entities;
using RL.Telemetry;

namespace RL.Enemies
{
    /// LichLight
    public class BeamWeak : Enemy, IDamageable
    {
        [SerializeField] GameObject barrier;

        [Header("Enemy Parameters")]
        public float barrierDamage = 20f; // Damage dealt on contact
        public float moveSpeed = 3f; // Movement speed

        [Header("Detection Parameters")]
        public float detectionRadius = 5f;
        public float detectionAngle = 45f;
        public LayerMask detectionMask;

        [Header("Barrier Parameters")]
        public float barrierCooldown = 3f; // Cooldown time for the barrier
        public float barrierDuration = 8f; // Duration for which the barrier remains active
         public float barrierChargeTime = 1f;  // Time spent idling before barrier activates

        float barrierInterval;
        public float minBarrierInterval = 0f; // Minimum time for additional charging
        public float maxBarrierInterval = 2f; // Maximum time for additional charging
        [SerializeField] bool _canChargeBarrier = true;
        [SerializeField] bool _isChargingBarrier = false;
        [SerializeField] bool _isUsingBarrier = false;


        [SerializeField] LichLightStateMachine stateMachine;
        public StateMachine<LichLightStates> sm => stateMachine;
        [SerializeField] LichLightAnimator animator;

        protected override void Start()
        {
            base.Start();
            barrier.SetActive(false);
            sm.OnStateChanged += animator.StateChangedCallback;
            sm.ToState(LichLightStates.Idle);
        }

        protected override void FixedUpdate()
        {
            Search();
            LookAtTarget();           
            if (!_isChargingBarrier)
            {
                MaintainDistance();
            }
            if (!_isUsingBarrier)
            {
                DetectProjectiles();
            }
            UpdateStates();
        }

        void UpdateStates()
        {
            if (target != null)
            {
                if (!_isChargingBarrier)
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
                    player.TakeDamage(barrierDamage);
                }
            }
        }

        void Update()
        {
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

        void DetectProjectiles()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 directionToCollider = collider.transform.position - transform.position;
                float angle = Vector2.Angle(transform.up, directionToCollider);

                if (angle <= detectionAngle)
                {
                    if (_canChargeBarrier)
                    {
                        ChargeBarrier();
                        StartCoroutine(Cooldown());
                    }
                }
            }
        }

        IEnumerator Cooldown()
        {
            _canChargeBarrier = false;
            yield return new WaitForSeconds((barrierCooldown + barrierInterval));
            _canChargeBarrier = true;
        }

        /// <summary>
        /// -1 (left)
        /// 0 (random)
        /// 1 (right)
        /// </summary>
        void MoveTowardsTarget()
        {
            if (target != null)
            {
                sm.ToState(LichLightStates.Tank);
                Game.Telemetry.GameStats[StatKey.EnemyAttackCount].Increment();
                Vector2 direction = (target.transform.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;
            }
        }

        void ChargeBarrier()
        {
            sm.ToState(LichLightStates.Idle);
            rb.velocity = Vector2.zero;
            StartCoroutine(BarrierCoroutine());
        }

        IEnumerator BarrierCoroutine()
        {
            barrierInterval = UnityEngine.Random.Range(minBarrierInterval, maxBarrierInterval);
            if (target == null) yield return null;
            {
            _isUsingBarrier = true;
            barrier.SetActive(true);
            sm.ToState(LichLightStates.Barrier);
            
            yield return new WaitForSeconds(barrierChargeTime);

            MoveTowardsTarget();
            yield return new WaitForSeconds(barrierDuration);

            barrier.SetActive(false);
            _isUsingBarrier = false;
            }
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
