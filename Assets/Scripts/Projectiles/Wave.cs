using UnityEngine;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Wave : Projectile
    {
        public float DissipateTime = 0.25f;
        bool _hasHit;
        
        protected override void Start()
        {
            base.Start();
            Owner.Stats.Stats.UseCountWave++;
        }

        public void Dissipate()
        {
            rb.velocity *= 0.05f;
            GetComponent<Collider2D>().enabled = false;
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

        void OnTriggerEnter2D(Collider2D other)
        {
            var go = other.gameObject;
            if (go.CompareTag("Enemy"))
            {
                if (go.TryGetComponent(out IDamageable hit))
                {                    
                    hit.TakeDamage(Data.Damage);
                    if (!_hasHit)
                    {
                        _hasHit = true;
                        Owner.Stats.Stats.HitCountWave++;
                    }
                }
            }
        }
    }
}