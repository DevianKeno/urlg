/*

Program Title: Fire Weak [Enemy AI] (Armadillo Enemy)

Date written: October 12, 2024
Date revised: November 8, 2024

Programmer/s:
    John Franky Nathaniel V. Batisla-Ong, Gian Paolo Buenconsejo

Where the program fits in the general system design:
    Part of the testbed platform (or Game module), the enemies and its AI.

Purpose:
    This script defines the behavior and mechanics of the Armadillo enemy, it's AI basically.
    It manages the enemy's attributes, states, interactions with the player, and its attack mechanics
    like its invincibility lunge and burn vulnerability.

Control:
    If spawned, the enemy remains idle until the player has entered the room
    in which it is located. If it does detect, it will indicate that it will
    attack and then proceed to become invincible as it turns into a ball, it 
    will then lunge at  the player's location. Afterwards, it will maintain 
    a set distance from the player, while trying to avoid the projectiles up 
    until sufficient time has passed and it can lunge once again.

Data Structures/Key Variables:
    StateMachine: controls the state transitions of the Armadillo
    [Definitions are found at their respective declarations]
*/

using System.Collections;

using UnityEngine;

using RL.Systems;
using RL.Entities;
using RL.Player;

namespace RL.Enemies
{
    /// <summary>
    /// Represents the "Armadillo" enemy, which specializes in being invincible in its attacks.
    /// </summary>
    public class FireWeak : Enemy, IDamageable, IBurnable
    {
        [Header("Enemy Parameters")]
        public float ContactDamage = 10f; // Damage inflicted upon player contact
        public float BurnTime = 3f; // Duration of burn when hit by a "Fire"-type projectile

        [Header("Detection Parameters")]
        public float detectionRadius = 5f; // Radius for detecting player/projectiles
        public float detectionAngle = 45f; // Angle range for detection

        /// <summary>
        /// Filter layers where it can only detect. Set to 'Player'.
        /// </summary>
        public LayerMask detectionMask;

        [Header("Lunge Parameters")]
        public float lungeForce = 500f; // Force applied when lunging
        public float lungeDistance = 200f; // Maximum distance covered in a lunge
        public float lungeCooldown = 1f; // Cooldown between lunge attacks
        bool _canLunge = true;
        bool _isLunging;
        public bool IsLunging => _isLunging;
        bool _hasHitOnce = false;

        [Header("Invincibility Parameters")]
        public float invincibilityDuration = 1f; // Time spent in the "ball" invincible form before lunging
        public float minLungeInterval = 1f; // Minimum time between lunge attacks
        public float maxLungeInterval = 3f; // Maximum time between lunge attacks
        public float lungeWindup = 0.5f; // Short delay before lunging

        bool _isInvincible = false;
        bool _isCharging = false;
        float lungeInterval;
        float lungeDelta;

        bool isBurning = false;

        GameObject burnParticle;

        [SerializeField] ArmadilloStateMachine stateMachine; // Handles AI state transitions
        public StateMachine<ArmadilloStates> sm => stateMachine;
        [SerializeField] ArmadilloAnimator animator; // Controls enemy animations

    /// <summary>
    /// Initializes the enemy
    /// </summary>
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
            if (IsAsleep) return; // If enemy is inactive, skip logic
            
            Search(); // Searches for player
            LookAtTarget(); // Adjusts orientation towards the player 

            if (!_isLunging && !_isCharging)
            {
                MaintainDistance(); // Keeps enemy at an optimal distance from player
            }

            UpdateStates(); // Updates AI state
        }

        protected override void OnFireTick()
        {
            /// Burn deals more damage on Armadillo
            TakeDamage(Game.BurnDamage * 3f);
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

    /// <summary>
    /// Handles process of it attacking
    /// </summary>
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

    /// <summary>
    /// Displays that the burning has stopped
    /// </summary>
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
