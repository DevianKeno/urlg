using UnityEngine;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Fireball : Projectile
    {
        protected override void Start()
        {
            base.Start();
            Owner.Stats.Stats.UseCountFire++;
        }

        void LateUpdate()
        {
            spriteRenderer.transform.rotation = Quaternion.identity;
        }

        protected override void OnHitEnemy(IDamageable hit)
        {
            hit.TakeDamage(Data.Damage);
            Owner.Stats.Stats.HitCountFire++;
            Destroy(gameObject);
        }
        
        protected override void OnHitShield(GameObject obj)
        {
            if (obj.TryGetComponent(out IBurnable burnable))
            {
                Debug.Log("burning");
                burnable.Burn();
            }
            Destroy(gameObject);
        }
    }
}

