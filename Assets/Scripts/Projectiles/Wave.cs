using UnityEngine;
using RL.Entities;
using RL.Levels;
using RL.Telemetry;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Wave : Projectile
    {
        public float DissipateTime = 0.25f;

        bool hasParticle;
        bool _hasHit;
        
        protected override void Start()
        {
            base.Start();
            
            Game.Telemetry.PlayerStats[StatKey.UseCountWave].Increment();
            Game.Audio.PlaySound("wave_shoot");
        }

        protected override void OnHitTile(GameObject obj, Collision2D collision)
        {
            if (obj.TryGetComponent<Tile>(out var tile))
            {
                if (tile is Glass glass)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        protected override void OnHitEnemy(IDamageable hit, Collision2D collision)
        {
            
        }

        public void Dissipate()
        {
            GetComponent<Collider2D>().enabled = false;
            rb.velocity *= 0.02f;
            var sr = GetComponent<SpriteRenderer>();
            LeanTween.value(gameObject, 1f , 0f, DissipateTime)
                .setOnUpdate((float i) =>
                {
                    var color = sr.color;
                    color.a = i;
                    sr.color = color;
                })
                .setEase(LeanTweenType.easeOutSine)
                .setOnComplete(() =>
                {
                    Destroy(gameObject);
                });
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            var go = other.gameObject;
            if (go.CompareTag("Enemy"))
            {
                if (go.TryGetComponent(out IDamageable hit))
                {
                    if (hit is WaveWeak ww) /// salaman
                    {
                        /// take double damage
                        hit.TakeDamage(Data.Damage);
                        CreatePuffParticle(ww.transform.position);
                    }

                    if (hit is FireWeak) /// armadill
                    {
                        Dissipate();
                    }

                    hit.TakeDamage(Data.Damage);
                    
                    if (!_hasHit)
                    {
                        _hasHit = true;
                        Game.Telemetry.PlayerStats[StatKey.HitCountWave].Increment();
                    }
                }
            }
        }
        
        void CreatePuffParticle(Vector3 position)
        {
            if (hasParticle) return;
            hasParticle = true;

            var puff = Game.Particles.Create("wave_puff");
            puff.transform.position = position;
        }
    }
}