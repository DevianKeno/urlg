using UnityEngine;
using RL.Enemies;
using RL.Systems;
using RL.Levels;

namespace RL.Projectiles
{
    public class Fireball : Projectile
    {
        protected override void Start()
        {
            base.Start();
            Owner.Stats.Stats.UseCountFire++;
            Game.Telemetry.PlayerStats["useCountFire"].Increment();
            Game.Audio.PlaySound("fire_shoot");
        }

        void LateUpdate()
        {
            spriteRenderer.transform.rotation = Quaternion.identity;
        }

        protected override void OnHitTile(GameObject obj)
        {
            if (obj.TryGetComponent<Tile>(out var tile))
            {
                if (tile is BurnableCrate crate)
                {
                    crate.StartBurning();
                    Destroy(gameObject);
                }
            }
        }

        protected override void OnHitEnemy(IDamageable hit)
        {
            hit.TakeDamage(Data.Damage);
            Owner.Stats.Stats.HitCountFire++;
            Game.Telemetry.PlayerStats["hitCountFire"].Increment();
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

