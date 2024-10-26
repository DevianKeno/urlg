using UnityEngine;
using RL.Entities;
using RL.Systems;
using RL.Levels;
using RL.Telemetry;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Fireball : Projectile
    {
        public float DissipateTime = 0.25f;

        protected override void Start()
        {
            base.Start();
            
            Game.Telemetry.PlayerStats[StatKey.UseCountFire].Increment();
            Game.Audio.Play("fire_shoot");
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
            // rb.velocity *= 0.05f;
            GetComponent<Collider2D>().enabled = false;
            var flamePrefab = Resources.Load<GameObject>("Prefabs/Embers");
            Instantiate(flamePrefab, transform);
            LeanTween.value(gameObject, 1f , 0f, DissipateTime)
                .setOnUpdate((float i) =>
                {
                    var color = spriteRenderer.color;
                    color.a = i;
                    spriteRenderer.color = color;
                    transform.localScale = new(i, i, 1f);
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
            
            bool registerHit = true;
            if (hit is Enemy enemy)
            {
                if (enemy.IsAsleep)
                {
                    Game.Audio.Play("bump");
                    registerHit = false;
                }
            }
            
            if (hit is IBurnable burnable)
            {
                if (hit is FireWeak armadil)
                {
                    if (armadil.IsLunging)
                    {
                        Dissipate();
                    }
                    else
                    {
                        burnable.Burn();
                    }
                }
                else
                {
                    burnable.Burn();
                }
            }

            if (registerHit)
            {
                Game.Telemetry.PlayerStats[StatKey.HitCountFire].Increment();
            }
            
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

