using UnityEngine;

using RL.Entities;
using RL.Levels;
using RL.Telemetry;
using RL.Enemies;

namespace RL.Projectiles
{
    public class Beam : Projectile
    {
        Vector2 _initialVelocity;

        protected override void Start()
        {
            base.Start();

            _initialVelocity = rb.velocity;

            Game.Telemetry.PlayerStats[StatKey.UseCountBeam].Increment();
            Game.Audio.Play("beam");
        }

        bool hasParticle;

        protected override void OnHitTile(GameObject obj, Collision2D collision)
        {
            if (obj.TryGetComponent<Tile>(out var tile))
            {
                if (tile is BurnableCrate crate)
                {
                    if (collision.contacts.Length > 0)
                    {
                        crate.TakeDamage(10);
                        CreatePuffParticle(collision.contacts[0].point);
                        Destroy(gameObject);
                        return;
                    }
                }
                else if (tile is Glass glass)
                {
                    if (collision.contacts.Length > 0)
                    {
                        Reflect(collision.contacts[0].normal);
                        // CreatePuffParticle(collision.contacts[0].point);
                        return;
                    }
                }
                
                if (collision.contacts.Length > 0)
                {
                    CreatePuffParticle(collision.contacts[0].point);
                }
                Game.Audio.Play("beam_hit");
                Destroy(gameObject);
            }
        }

        bool hadReflected;

        void CreatePuffParticle(Vector3 position)
        {
            if (hasParticle) return;
            hasParticle = true;

            var puff = Game.Particles.Create("beam_puff");
            puff.transform.position = position;
        }

        void Reflect(Vector3 surfaceNormal)
        {
            Game.Audio.Play("beam_reflect");
            // if (hadReflected) return;

            // var duplicateBeam = Instantiate(gameObject);
            // duplicateBeam.transform.position = transform.position + new Vector3(_initialVelocity.x, _initialVelocity.y);

            /// Reflect the velocity based on the surface normal
            Vector3 newDirection = Vector3.Reflect(_initialVelocity, surfaceNormal);
            rb.velocity = newDirection;
            _initialVelocity = newDirection;

            /// Split beam

            /// Rotate the projectile to match the new direction
            float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            hadReflected = true;
        }

        protected override void OnHitEnemy(IDamageable hit, Collision2D collision)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;

            bool registerHit = true;
            if (hit is Enemy enemy)
            {
                if (enemy.IsAsleep)
                {
                    Game.Audio.Play("bump");
                    registerHit = false;
                }
            }

            if (hit is FireWeak) /// armadil
            {
                if (collision.contacts.Length > 0)
                {
                    Reflect(collision.contacts[0].normal);
                    CreatePuffParticle(collision.contacts[0].point);
                    return;
                }
            }
            else if (hit is BeamWeak)
            {
                Game.Audio.Play("hitmarker");
                CreatePuffParticle(collision.contacts[0].point);
                hit.TakeDamage(Data.Damage * 3);
            }
            else if (hit is WaveWeak)
            {
                CreatePuffParticle(collision.contacts[0].point);
                hit.TakeDamage(Data.Damage);
            }
            
            if (registerHit)
            {
                Game.Telemetry.PlayerStats[StatKey.HitCountBeam].Increment();
            }
            
            Destroy(gameObject);
        }

        protected override void OnHitShield(GameObject obj)
        {
            Destroy(gameObject);
        }
    }
}