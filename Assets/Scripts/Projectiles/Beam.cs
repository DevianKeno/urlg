using UnityEngine;
using RL.Entities;
using RL.Levels;
using RL.Telemetry;

namespace RL.Projectiles
{
    public class Beam : Projectile
    {
        protected override void Start()
        {
            base.Start();

            Game.Telemetry.PlayerStats[StatKey.UseCountBeam].Increment();
            Game.Audio.PlaySound("beam_shoot");
        }

        protected override void OnHitTile(GameObject obj)
        {
            if (obj.TryGetComponent<Tile>(out var tile))
            {
                if (tile is BurnableCrate crate)
                {
                    crate.TakeDamage(25);
                    Destroy(gameObject);
                }
                if (tile is Glass glass)
                {
                    /// Reflect this beam
                    return;
                }
                Destroy(gameObject);
            }
        }

        protected override void OnHitEnemy(IDamageable hit)
        {
            hit.TakeDamage(Data.Damage);
            Game.Telemetry.PlayerStats[StatKey.HitCountBeam].Increment();
            Destroy(gameObject);
        }

        protected override void OnHitShield(GameObject obj)
        {
            Destroy(gameObject);
        }
    }
}