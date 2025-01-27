using UnityEngine;

using RL.Entities;
using RL.Levels;
using RL.Telemetry;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Fireball : Projectile
    {
        public float DissipateTime = 0.25f;
        public float SpeedUpTime = 2f;
        public float SpeedIncreasePercent = 2f;

        bool _isSpeedingUp = false;
        float _elapsedSpeedUpTime = 0f;
        Vector3 finalVelocity;
    
        protected override void Start()
        {
            base.Start();
            
            Game.Telemetry.PlayerStats[StatKey.UseCountFire].Increment();
            Game.Audio.Play("fire_shoot");

            finalVelocity = rb.velocity * SpeedIncreasePercent;
        }

        void LateUpdate()
        {
            if (_isSpeedingUp)
            {
                _elapsedSpeedUpTime += Time.deltaTime;
                float t = Mathf.Clamp01(_elapsedSpeedUpTime / SpeedUpTime);
                rb.velocity = Vector3.Lerp(rb.velocity, finalVelocity, t);

                if (_elapsedSpeedUpTime >= SpeedUpTime)
                {
                    rb.velocity = finalVelocity;
                    _isSpeedingUp = false;
                }
            }
            spriteRenderer.transform.rotation = Quaternion.identity;
        }
        
        protected override void OnHitTile(GameObject obj, Collision2D collision)
        {
            if (obj.TryGetComponent<Tile>(out var tile))
            {
                if (tile is BurnableCrate burnable)
                {
                    burnable.TakeDamage(Data.Damage);
                    burnable.StartBurning(99f);
                    CreateEmbers();
                    Game.Audio.Play("fire_burst");
                    Destroy(gameObject);
                }
            }
        }

        public void Dissipate()
        {
            // rb.velocity *= 0.05f;
            Game.Audio.Play("fizz");
            GetComponent<Collider2D>().enabled = false;
            CreateEmbers();

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

        void CreateEmbers()
        {
            var flamePrefab = Instantiate(Resources.Load<GameObject>("Prefabs/Embers"), transform);
            flamePrefab.transform.SetParent(null, worldPositionStays: true);
        }

        protected override void OnHitEnemy(IDamageable hit, Collision2D collision)
        {
            bool registerHit = true;
            if (hit is Enemy enemy)
            {
                if (enemy.IsAsleep)
                {
                    Game.Audio.Play("bump");
                    registerHit = false;
                }
                else
                {
                    Game.Audio.Play("fire_burst");
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
                        hit.TakeDamage(Data.Damage);
                        burnable.Burn(99f);
                    }
                }
                else if (hit is BeamWeak lich)
                {
                    lich.TakeDamage(Data.Damage);
                    /// Lich does not burn
                }
                else if (burnable is BurnableCrate crate)
                {
                    crate.TakeDamage(Data.Damage);
                    if (!crate.IsBurning)
                    {
                        burnable.Burn(99f);
                    }
                    CreateEmbers();
                    Game.Audio.Play("fire_burst");
                }
                else
                {
                    hit.TakeDamage(Data.Damage);
                    burnable.Burn(3f);
                    CreateEmbers();
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
                burnable.Burn(99f);
            }
            Dissipate();
        }
    }
}

