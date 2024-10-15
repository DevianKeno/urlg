using System;
using System.Collections;
using UnityEngine;
using URLG.Systems;
using URLG.Player;
using URLG.Enemies;
using Random = UnityEngine.Random;

namespace URLG.Enemies
{
    /// Armadillo (FireWeak)
    public class FireWeak : Enemy, IDamageable
    {
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

        [Header("Invincibility Parameters")]
        public float invincibilityDuration = 2f; // Time spent in the "ball" invincible form before lunging
        public float minLungeInterval = 2f;
        public float maxLungeInterval = 5f;
        public float lungeWindup = 0.5f; // Short delay before lunging

        bool _isInvincible = false;
        bool _isCharging = false;
        float lungeInterval;
        float lungeDelta;

        [SerializeField] ArmadilloStateMachine stateMachine;
        public StateMachine<ArmadilloStates> sm => stateMachine;
        [SerializeField] ArmadilloAnimator animator;

        protected override void Start()
        {
            base.Start();
            lungeInterval = Random.Range(minLungeInterval, maxLungeInterval);
            lungeDelta = 0f;
            sm.OnStateChanged += animator.StateChangedCallback;
            sm.ToState(ArmadilloStates.Idle);
        }

        protected override void FixedUpdate()
        {
            Search();
            LookAtTarget();

            if (!_isLunging && !_isCharging)
            {
                MaintainDistance();
            }

            UpdateStates();
        }

        void UpdateStates()
        {
            if (target != null)
            {
                if (!_isCharging)
                {
                    sm.ToState(ArmadilloStates.Move);
                }
            }
            else
            {
                sm.ToState(ArmadilloStates.Idle);
            }
        }

        void Update()
        {
            lungeDelta += Time.deltaTime;

            if (lungeDelta >= lungeInterval && target != null && _canLunge)
            {
                StartCoroutine(InvincibilityLunge());
            }
        }

        IEnumerator InvincibilityLunge()
        {
            _canLunge = false;
            _isCharging = true;

            sm.ToState(ArmadilloStates.Ball); // State where the enemy turns into a ball
            _isInvincible = true;

            // Invincibility phase before lunging
            yield return new WaitForSeconds(invincibilityDuration);

            sm.ToState(ArmadilloStates.Windup);
            yield return new WaitForSeconds(lungeWindup);

            _isInvincible = false; // End invincibility
            Lunge(); // Perform the lunge attack

            yield return new WaitForSeconds(lungeCooldown);
            lungeInterval = Random.Range(minLungeInterval, maxLungeInterval);
            lungeDelta = 0f;
            _canLunge = true;
            _isCharging = false;
        }

        void Lunge()
        {
            _isLunging = true;

            Vector2 lungeDirection = (target.transform.position - transform.position).normalized;
            rb.AddForce(lungeDirection * lungeForce, ForceMode2D.Impulse);

            sm.ToState(ArmadilloStates.Lunge);
            sm.LockFor(0.5f);

            StartCoroutine(ResetLunge());
        }

        IEnumerator ResetLunge()
        {
            yield return new WaitForSeconds(0.5f);
            _isLunging = false;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
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
