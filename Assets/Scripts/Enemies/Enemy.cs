/*

Component Title: Enemy (Base)
Last updated: November 8, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component is the base class for all 'Enemy' entities in the game.

Data Structures:
    [Definitions are found at their respective declarations]
*/

using System;

using RL.Player;
using RL.UI;
using UnityEngine;

namespace RL.Entities
{    
    public class Enemy : Entity, IDamageable, IBurnable
    {
        /// <summary>
        /// The current health of this entity.
        /// </summary>
        public float Health = 100f;
        public float MoveSpeed = 2f;
        public Color DamageFlash = Color.red;
        
        /// <summary>
        /// A 'sleeping' entity will not be able to interact with anything.
        /// </summary>
        public bool IsAsleep { get; set; } = true; 
        bool hasTarget; 

        [Header("Search Parameters")]
        public float searchRadius = 5f;
        public float rotationSpeed = 50f;
        
        [Header("Follow Parameters")]
        /// <summary>
        /// Desired distance from the player.
        /// </summary>
        public float followDistance = 5f;
        /// <summary>
        /// Speed at which the enemy follows the player.
        /// </summary>
        public float followSpeed = 2f;
        /// <summary>
        /// Amount of damping effect for smoothed movement.
        /// </summary>
        public float followDamping = 0.5f;
        /// <summary>
        /// Maximum cone angle amount for strafing left/right.
        /// </summary>
        public float strafeAngle = 30f;
        /// <summary>
        /// Speed at which to strafe.
        /// </summary>
        public float strafeSpeed = 1f;
        /// <summary>
        /// Must be ideally only be -1 or 1, representing the direction at which to strafe.
        /// </summary>
        float strafeDirection;

        #region 
        /// <summary>
        /// Called when this entity dies.
        /// </summary>
        public event Action<Enemy> OnDeath;
        #endregion

        protected Color prevColor;
        protected GameObject target;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected HealthBar healthBar;

        protected virtual void Start()
        {
            prevColor = spriteRenderer.color;
            rb = GetComponent<Rigidbody2D>();
            if (healthBar == null)
            {
                healthBar = Game.UI.Create<HealthBar>("Enemy HP Bar", parent: transform);
                healthBar.transform.SetAsLastSibling();
                healthBar.MaximumHealth = Health;
                healthBar.ActualHealth = Health;
            }
            ChooseNewStrafeDirection();
            InvokeRepeating(nameof(ChooseNewStrafeDirection), 2f, 2f); // Change strafe direction every 2 seconds
        }

        protected virtual void FixedUpdate()
        {
            if (IsAsleep) return;

            Search();
            LookAtTarget();
            MaintainDistance();
        }

        protected virtual void LateMovement()
        {
        }

        /// <summary>
        /// OnFire component.
        /// </summary>
        protected OnFire onFire;

        /// <summary>
        /// Burns this enemy for a specified duration.
        /// </summary>
        public virtual void Burn(float duration)
        {
            if (onFire == null)
            {
                onFire = gameObject.AddComponent<OnFire>();
                onFire.OnTick += OnFireTick;
            }
            
            if (!onFire.IsBurning)
            {
                onFire.StartBurn(duration);
            }
        }

        protected virtual void OnFireTick()
        {
            TakeDamage(Game.BurnDamage);
        }

        /// <summary>
        /// Searches for player targers within a predefined radius.
        /// </summary>
        protected virtual void Search()
        {
            if (hasTarget) return;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    target = collider.gameObject;
                    hasTarget = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Set a target player setting the stage for further actions.
        /// </summary>
        public void SetTargetPlayer(PlayerController player)
        {
            if (player != null)
            {
                target = player.gameObject;
                hasTarget = true;
            }
        }

        /// <summary>
        /// Looks at the current target (if any).
        /// </summary>
        protected virtual void LookAtTarget()
        {
            if (!hasTarget) return;
            
            Vector3 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        /// <summary>
        /// Maintains its preferred distance from the current target (if any).
        /// </summary>
        protected virtual void MaintainDistance()
        {
            if (!hasTarget) return;

            var t = target.transform;
            Vector2 directionToPlayer = (t.position - transform.position).normalized;
            Vector2 desiredPosition = (Vector2)t.position - directionToPlayer * followDistance;
            Vector2 smoothedPosition = Vector2.Lerp(rb.position, desiredPosition, followDamping * Time.fixedDeltaTime);

            Vector2 strafeOffset = new(
                Mathf.Cos(strafeDirection) * MoveSpeed * Time.fixedDeltaTime,
                Mathf.Sin(strafeDirection) * MoveSpeed * Time.fixedDeltaTime
            );
            smoothedPosition += strafeOffset;
            rb.MovePosition(smoothedPosition);
        }

        /// <summary>
        /// Randomly chooses a direction on which to strafe at.
        /// </summary>
        protected virtual void ChooseNewStrafeDirection()
        {
            float randomAngle = UnityEngine.Random.Range(-strafeAngle, strafeAngle);
            strafeDirection = Mathf.Deg2Rad * randomAngle; // Convert angle to radians for trigonometric functions
        }

        /// <summary>
        /// Take damage reducing this entity's health.
        /// </summary>
        public virtual void TakeDamage(float amount)
        {
            if (IsAsleep) return;
            
            Flash();
            Health -= amount;
            if (Health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Kill this entity.
        /// </summary>
        public virtual void Die()
        {
            /// Does not die if player is dead first
            if (Game.Main.Player != null && !Game.Main.Player.IsAlive) return;

            OnDeath?.Invoke(this);
            var puffParticle = Game.Particles.Create("puff");
            puffParticle.transform.position = transform.position;
            Game.Audio.Play("puff");
            Destroy(gameObject);
        }

        /// <summary>
        /// Simulates a damage flash effect.
        /// </summary>
        public virtual void Flash()
        {
            spriteRenderer.color = prevColor;
            LeanTween.cancel(gameObject);
            spriteRenderer.color = DamageFlash;
            LeanTween.value(gameObject, DamageFlash, prevColor, 0.25f)
                .setOnUpdate((Color i) =>
                {
                    spriteRenderer.color = i;
                })
                .setEase(LeanTweenType.easeOutSine);
        }

        protected virtual void OnDrawGizmos()
        {
            // Search radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
    }
}
