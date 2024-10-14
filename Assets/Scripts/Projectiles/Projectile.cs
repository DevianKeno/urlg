using System;
using UnityEngine;
using RL.Player;
using RL.Entities;
using RL.Levels;

namespace RL.Projectiles
{
    public abstract class Projectile : MonoBehaviour
    {
        [SerializeField] protected ProjectileData projectileData;
        public ProjectileData Data => projectileData;
        [SerializeField] protected PlayerController owner;
        public PlayerController Owner => owner;

        [SerializeField] protected Rigidbody2D rb;
        public Rigidbody2D Rigidbody2D => rb;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        protected virtual void Start()
        {
            Destroy(gameObject, Data.DespawnAfter);
        }

        public void SetOwner(PlayerController playerController)
        {
            owner = playerController;
        }

        public virtual void SetDirection(Vector2 direction)
        {
            rb.velocity = direction * Data.Speed;
        }

        protected virtual void OnHitTile(GameObject obj, Collision2D collision)
        {
        }

        protected virtual void OnHitEnemy(IDamageable hit)
        {
        }

        protected virtual void OnHitShield(GameObject obj)
        {
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Shield"))
            {
                OnHitShield(other.gameObject);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject == null) return;
            
            foreach (ContactPoint2D contact in collision.contacts)
            {
                var go = contact.collider.gameObject;

                if (go.CompareTag("Tile"))
                {
                    OnHitTile(go, collision);
                    break;

                } else if (go.CompareTag("Enemy"))
                {
                    if (go.TryGetComponent(out IDamageable hit))
                    {
                        OnHitEnemy(hit);
                        break;
                    }

                } else if (go.CompareTag("Shield"))
                {
                    OnHitShield(go);
                    break;
                }
            }
        }
    }
}