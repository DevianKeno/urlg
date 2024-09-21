using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace URLG.Enemies
{    
    public class Enemy : MonoBehaviour, IDamageable
    {
        public float Health = 100f;
        public float MoveSpeed = 2f;
        public WeaponType WeakType;
        public Color DamageFlash = Color.red;

        [Header("Search Parameters")]
        public float searchRadius = 5f;
        public float rotationSpeed = 50f;
        
        [Header("Follow Parameters")]
        public float followDistance = 5f; // Desired distance from the player
        public float followSpeed = 2f; // Speed at which the enemy follows the player
        public float followDamping = 0.5f; // Damping effect for smooth movement
        public float strafeAngle = 30f; // Maximum cone angle for strafing
        public float strafeSpeed = 1f; // Speed at which the enemy strafes
        float strafeDirection;

        protected Color prevColor;
        protected GameObject target;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Rigidbody2D rb;

        protected Vector2 frameMovement;

        protected virtual void Start()
        {
            prevColor = spriteRenderer.color;
            rb = GetComponent<Rigidbody2D>();
            ChooseNewStrafeDirection();
            InvokeRepeating(nameof(ChooseNewStrafeDirection), 2f, 2f); // Change strafe direction every 2 seconds
        }

        protected virtual void FixedUpdate()
        {            
            Search();
            LookAtTarget();
            MaintainDistance();
        }

        protected virtual void LateMovement()
        {
        }

        protected virtual void Search()
        {
            if (target != null) return;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    target = collider.gameObject;
                    break;
                }
            }
        }

        protected virtual void LookAtTarget()
        {
            if (target == null) return;
            
            Vector3 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        protected virtual void MaintainDistance()
        {
            if (target == null) return;

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

        protected virtual void ChooseNewStrafeDirection()
        {
            float randomAngle = Random.Range(-strafeAngle, strafeAngle);
            strafeDirection = Mathf.Deg2Rad * randomAngle; // Convert angle to radians for trigonometric functions
        }

        public virtual void TakeDamage(float damage)
        {
            Flash();
            Health -= damage;
            if (Health <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            var puffParticle = Game.Particles.Create("puff");
            puffParticle.transform.position = transform.position;
            Destroy(gameObject);
        }

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
