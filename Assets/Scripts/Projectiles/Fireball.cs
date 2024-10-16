using UnityEngine;
using RL.Entities;
using RL.Systems;
using RL.Levels;
using RL.Telemetry;

namespace RL.Projectiles
{
    public class Fireball : Projectile
    {
        public float DissipateTime = 0.25f;

        protected override void Start()
        {
            base.Start();
            
            Game.Telemetry.PlayerStats[StatKey.UseCountFire].Increment();
            Game.Audio.PlaySound("fire_shoot");
        }

        void LateUpdate()
        {
            spriteRenderer.transform.rotation = Quaternion.identity;
        }

        protected override void OnHitTile(GameObject obj, Collision2D collision)
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

        public void Dissipate()
        {
            rb.velocity *= 0.05f;
            GetComponent<Collider2D>().enabled = false;
            var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
            Instantiate(flamePrefab, transform);
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

        protected override void OnHitEnemy(IDamageable hit, Collision2D collision)
        {
            hit.TakeDamage(Data.Damage);
            
            Game.Telemetry.PlayerStats[StatKey.HitCountFire].Increment();
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

