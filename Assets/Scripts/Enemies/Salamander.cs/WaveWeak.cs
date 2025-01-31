/*

Program Title: Wave Weak [Enemy AI] (Salamander)
Date written: October 4, 2024
Date revised: October 29, 2024

Programmer/s:
    John Franky Nathaniel V. Batisla-Ong, Gian Paolo Buenconsejo

Purpose:
    This is the main script for the enemy named "Salamander" enemy.
    The "Salamander" is an enemy that is weak to "Wave"-type of attacks.
    This implementation is designed so that the enemy can: 
    - Detect and attack the player via lunging.
    - Maintain a set distance from the player after said lunge.
    - Transition between different AI states using a state machine.

Control:
    If spawned, the enemy remains idle until the player has entered the room
    in which it is located. If it does detect, it will indicate that it will
    attack and then proceed to lunge at the player's location, this lunge will
    shield it from most projectiles except the "Wave"-type. Afterwards, it will
    maintain a set distance from the player, while trying to avoid the projectiles
    up until sufficient time has passed and it can lunge once again.

Data Structures/Key Variables:
    SalamanderStateMachine: Handles AI state transitions
    SalamanderAnimator animator: Controls enemy animations
    [Definitions are found at their respective declarations]
*/

using System;
using System.Collections;

using UnityEngine;

using RL.Systems;
using RL.Player;

namespace RL.Entities
{
    /// <summary>
    /// Represents the "WaveWeak" enemy type, a salamander-like creature
    /// </summary>
    public class WaveWeak : Enemy, IDamageable
    {
        [SerializeField] GameObject shield; // Shield object that activates during charge

        [Header("Enemy Parameters")]
        public float ContactDamage = 10f; // Damage inflicted when colliding with the player

        [Header("Detection Parameters")]
        public float detectionRadius = 5f; // Radius for detecting player/projectiles
        public float detectionAngle = 45f; // Angle range for detection
        public LayerMask detectionMask; // Specifies layers that can be detected

        [Header("Lunge Parameters")]
        public float lungeForce = 5f;  // Force applied when lunging
        public float lungeDistance = 2f; // Maximum distance covered in a lunge
        public float lungeCooldown = 1f; // Cooldown between lunge attacks
        bool _canLunge = true;
        bool _isLunging;

        [Header("Charge Parameters")]
        public float chargeSpeed = 10f; // Speed of the charge
        public float overshoot = 2f; // Force applied to overshoot the player
        public float chargeCooldown = 5f; // Cooldown between charge attacks
        public float minChargeInterval = 2f; // Minimum time between charge attacks
        public float maxChargeInterval = 2f; // Maximum time between charge attacks
        public float chargeWindup = 1f; // Duration of the windup before charging
        public float chargeDuration = 2f; // Duration of the charge attack
        float chargeInterval;
        float chargeDelta;
        [SerializeField] bool _canCharge = true;
        [SerializeField] bool _isCharging = false;

        [SerializeField] SalamanderStateMachine stateMachine; // Handles AI state transitions
        public StateMachine<SalamanderStates> sm => stateMachine;
        [SerializeField] SalamanderAnimator animator; // Controls enemy animations

    /// <summary>
    /// Initializes the enemy
    /// </summary>
        protected override void Start()
        {
            base.Start();
            chargeInterval = UnityEngine.Random.Range(minChargeInterval, maxChargeInterval);
            chargeDelta = 0f;
            shield.SetActive(false);
            sm.OnStateChanged += animator.StateChangedCallback;

            sm.ToState(SalamanderStates.Idle); // Starts in idle state
        }

        protected override void FixedUpdate()
        {
            if (IsAsleep) return; // If enemy is inactive, skip logic
            
            Search(); // Searches for player
            LookAtTarget();  // Adjusts orientation towards the player          
            if (!_isLunging && !_isCharging)
            {
                MaintainDistance(); // Keeps enemy at an optimal distance from player
            }
            if (!_isCharging) // Checks if enemy should react to projectiles
            {
                DetectProjectiles();  
            }
            UpdateStates(); // Updates AI state
        }

        /// <summary>
        /// Updates the enemy's AI state based on the presence of a target.
        /// </summary>
        void UpdateStates()
        {
            if (target != null) 
            {
                if (!_isCharging)
                {
                    sm.ToState(SalamanderStates.Move);
                }
            } else
            {
                sm.ToState(SalamanderStates.Idle);
            }
        }

        /// <summary>
        /// Handles damage dealing when the enemy collides with the player.
        /// </summary>
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

        /// <summary>
        /// Controls the charge attack behavior using a coroutine.
        /// </summary>
        IEnumerator ChargeCoroutine()
        {
            if (target == null) yield return null;

            _isCharging = true;
            sm.ToState(SalamanderStates.Charge);

            /// Windup before charging
            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(chargeWindup);

            Vector2 targetPosition = target.transform.position;
            Vector2 overshootDirection = (targetPosition - (Vector2) transform.position).normalized;
            Vector2 overshootPosition = targetPosition + (overshootDirection * overshoot);
            float startTime = Time.time;
            float journeyLength = Vector2.Distance(transform.position, overshootPosition);

            Game.Telemetry.IncrementEnemyAttackCount();
            shield.SetActive(true);
            sm.ToState(SalamanderStates.Jump);
            while (Time.time - startTime < chargeDuration)
            {
                float distanceCovered = (Time.time - startTime) * chargeSpeed;
                rb.MovePosition(Vector2.Lerp(transform.position, overshootPosition, EaseOutCubic(distanceCovered / journeyLength)));
                yield return null;
            }
            
            shield.SetActive(false);
            _isCharging = false;
            yield return new WaitForSeconds(chargeCooldown);
            chargeInterval = UnityEngine.Random.Range(minChargeInterval, maxChargeInterval);
            chargeDelta = 0f;
            _canCharge = true;
        }

    /// <summary>
    /// Allows the enemy to detect projectiles and lunge accordingly
    /// </summary>
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

    /// <summary>
    /// Initiates a period where the enemy cannot lunge
    /// </summary>
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
        /// Allows the enemy to lunge
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
            sm.ToState(SalamanderStates.Hop);
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
