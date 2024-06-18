using UnityEngine;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Beam : Projectile
    {
        protected override void Start()
        {
            base.Start();
            Game.Telemetry.PlayerStats["useCountBeam"].Increment();
        }

        protected override void OnHitWall(GameObject obj)
        {
            Destroy(gameObject);
        }

        protected override void OnHitEnemy(IDamageable hit)
        {
            hit.TakeDamage(Data.Damage);
            Game.Telemetry.PlayerStats["hitCountBeam"].Increment();
            Destroy(gameObject);
        }

        protected override void OnHitShield(GameObject obj)
        {
            Destroy(gameObject);
        }
    }
}