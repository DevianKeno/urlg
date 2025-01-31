/*

Program Title: Beam Weak [Enemy AI] (Lich Light)
Date written: October 4, 2024
Date revised: October 29, 2024

Programmer/s:
    John Franky Nathaniel V. Batisla-Ong, Gian Paolo Buenconsejo

Where the program fits in the general system design:
    Part of the testbed platform (or Game module), the enemies and its AI.

Purpose:
    This script defines the behavior and mechanics of the Lich Light enemy, it's AI basically.
    It manages the enemy's attributes, states, interactions with the player, and its attack mechanics
    like its supportive barrier lunge and beam vulnerability.

Control:
        If spawned, the enemy remains idle until the player has entered the room
    in which it is located. If it does detect, it will indicate that it will
    attack and then proceed to activate its barrier while moving towards the 
    player's location, this lunge will shield itself and other enemies from most 
    projectiles in a radius except the "Beam"-type. Afterwards, it will maintain 
    a set distance from the player, while trying to avoid the projectiles up until 
    sufficient time has passed and it can lunge once again.

Data Structures/Key Variables:
    LichLightStateMachine: Handles the state transitions of the Lich Light
    [Definitions are found at their respective declarations]
*/

using System;
using System.Collections;

using UnityEngine;

using RL.Systems;
using RL.Player;
using RL.Entities;

namespace RL.Enemies
{
    /// <summary>
    /// Represents the "Lich Light" enemy, which specializes in barrier-assisted attacks.
    /// </summary>
    public class BeamWeak : Enemy, IDamageable
    {
        [SerializeField] GameObject barrier; // Barrier that activates during the charge

        [Header("Enemy Parameters")]
        public float ContactDamage = 10f; // Damage inflicted upon player contact

        [Header("Detection Parameters")]
        public float detectionRadius = 5f; // Radius for detecting player/projectiles
        public float detectionAngle = 45f; // Angle range for detection
        public LayerMask detectionMask; // Specifies layers that can be detected
        
        [Header("Lunge Parameters")]
        public float lungeForce = 5f; // Force applied when lunging
        public float lungeDistance = 2f; // Maximum distance covered in a lunge
        public float lungeCooldown = 1f; // Cooldown between lunge attacks
        bool _canLunge = true;
        bool _isLunging;

        [Header("Charge Parameters")]
        public float chargeSpeed = 10f; // Speed of the charge
        public float overshoot = 2f; // Extra distance beyond player's position
        public float chargeCooldown = 5f; // Cooldown between charge attacks
        public float minChargeInterval = 2f; // Minimum time between charge attacks
        public float maxChargeInterval = 2f; // Maximum time between charge attacks
        public float chargeWindup = 1f; // Duration of the windup before charging
        public float chargeDuration = 2f; // Duration of the charge attack
        float chargeInterval;
        float chargeDelta;
        [SerializeField] bool _canCharge = true;
        [SerializeField] bool _isCharging = false;

        [SerializeField] LichLightStateMachine stateMachine; // Handles AI state transitions
        public StateMachine<LichLightStates> sm => stateMachine;
        [SerializeField] LichLightAnimator animator; // Controls enemy animations

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
            if (IsAsleep) return; // If enemy is inactive, skip logic
            
            Search(); // Searches for player
            LookAtTarget(); // Adjusts orientation towards the player             
            if (!_isLunging && !_isCharging)
            {
                MaintainDistance(); // Keeps enemy at an optimal distance from player
            }
            if (!_isCharging) 
            {
                DetectProjectiles(); // Checks if enemy should react to projectiles
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
                    sm.ToState(LichLightStates.Move);
                }
            } else
            {
                sm.ToState(LichLightStates.Idle);
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

    /// <summary>
    /// Flips the sprite to better represent the enemy while switching directions
    /// </summary>
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
            sm.ToState(LichLightStates.Move);

    /// <summary>
    /// Activating the barrier during the windup before charging
    /// </summary>
            rb.velocity = Vector2.zero;
            barrier.SetActive(true);
            yield return new WaitForSeconds(chargeWindup);

            Vector2 targetPosition = target.transform.position;
            Vector2 overshootDirection = (targetPosition - (Vector2) transform.position).normalized;
            Vector2 overshootPosition = targetPosition + (overshootDirection * overshoot);
            float startTime = Time.time;
            float journeyLength = Vector2.Distance(transform.position, overshootPosition);

            Game.Telemetry.IncrementEnemyAttackCount();
            sm.ToState(LichLightStates.Move);
            while (Time.time - startTime < chargeDuration)
            {
                float distanceCovered = (Time.time - startTime) * chargeSpeed;
                rb.MovePosition(Vector2.Lerp(transform.position, overshootPosition, EaseOutCubic(distanceCovered / journeyLength)));
                yield return null;
            }

    /// <summary>
    /// Disabling the barrier after charging
    /// </summary>
            barrier.SetActive(false);
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
