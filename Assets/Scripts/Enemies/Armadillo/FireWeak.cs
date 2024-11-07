using System;
using System.Collections;

using UnityEngine;

using RL.Systems;
using RL.Entities;
using RL.Telemetry;
using RL.Player;

namespace RL.Enemies
{
    /// Armadillo (FireWeak)
    public class FireWeak : Enemy, IDamageable, IBurnable
    {
        [Header("Enemy Parameters")]
        public float ContactDamage = 10f;
        public float BurnTime = 3f;

        [Header("Detection Parameters")]
        public float detectionRadius = 5f;
        public float detectionAngle = 45f;
        public LayerMask detectionMask;
        public float lungeForce = 500f;
        public float lungeDistance = 200f;
        public float lungeCooldown = 1f;
        bool _canLunge = true;
        bool _isLunging;
        public bool IsLunging => _isLunging;
        bool _hasHitOnce = false;

        [Header("Invincibility Parameters")]
        public float invincibilityDuration = 1f; // Time spent in the "ball" invincible form before lunging
        public float minLungeInterval = 1f;
        public float maxLungeInterval = 3f;
        public float lungeWindup = 0.5f; // Short delay before lunging

        bool _isInvincible = false;
        bool _isCharging = false;
        float lungeInterval;
        float lungeDelta;

        bool isBurning = false;

        GameObject burnParticle;

        [SerializeField] ArmadilloStateMachine stateMachine;
        public StateMachine<ArmadilloStates> sm => stateMachine;
        [SerializeField] ArmadilloAnimator animator;

        protected override void Start()
        {
            base.Start();
            lungeInterval = UnityEngine.Random.Range(minLungeInterval, maxLungeInterval);
            lungeDelta = 0f;
            sm.OnStateChanged += animator.StateChangedCallback;
            sm.ToState(ArmadilloStates.Idle);
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

            UpdateStates();
        }

        protected override void OnFireTick()
        {
            /// Burn deals more damage on Armadil
            TakeDamage(Game.BurnDamage * 3f);
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

            sm.ToState(ArmadilloStates.Windup); // State where the enemy turns into a ball
            _isInvincible = true;

            // Invincibility phase before lunging
            yield return new WaitForSeconds(lungeWindup);

            sm.ToState(ArmadilloStates.Ball);
            yield return new WaitForSeconds(invincibilityDuration);

            Lunge(); // Perform the lunge attack
            Game.Telemetry.IncrementEnemyAttackCount();

            yield return new WaitForSeconds(lungeCooldown);
            lungeInterval = UnityEngine.Random.Range(minLungeInterval, maxLungeInterval);
            lungeDelta = 0f;
            _canLunge = true;
            _isCharging = false;
        }

        void Lunge()
        {
            ExtinguishFire();
            _isLunging = true;
            _hasHitOnce = false;

            Vector2 lungeDirection = (target.transform.position - transform.position).normalized;
            rb.AddForce(lungeDirection * lungeForce, ForceMode2D.Impulse);

            sm.ToState(ArmadilloStates.Lunge);
            _isInvincible = false; // End invincibility
            sm.LockFor(0.5f);

            StartCoroutine(ResetLunge());
        }

        public void ExtinguishFire()
        {
            if (onFire != null && onFire.IsBurning)
            {
                onFire.StopBurn();
                Game.Audio.Play("fizz");
                var flamePrefab = Instantiate(Resources.Load<GameObject>("Prefabs/Embers"), transform);
                Destroy(flamePrefab, 0.25f);
            }
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
                    if (!_hasHitOnce)
                    {
                        _hasHitOnce = true;
                        player.TakeDamage(ContactDamage);
                    }
                }
            }
        }
    }
}
