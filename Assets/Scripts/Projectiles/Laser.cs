using UnityEngine;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Laser : Projectile
    {
        protected override void Start()
        {
            base.Start();
            Owner.Stats.Stats.UseCountBeam++;
        }

        protected override void OnHitWall(GameObject obj)
        {
            Destroy(gameObject);
        }

        protected override void OnHitEnemy(IDamageable hit)
        {
            hit.TakeDamage(Data.Damage);
            Owner.Stats.Stats.HitCountLaser++;
            Destroy(gameObject);
        }

        protected override void OnHitShield(GameObject obj)
        {
            Destroy(gameObject);
        }
    }
}