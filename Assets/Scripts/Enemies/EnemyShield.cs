/*
Component Title: Enemy Shield
Last updated: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component represents a shield worn by enemies.
    This shield blocks specific player projectiles that collide with it.
    This completely blocks the 'Beam' projectile, but is completely ignored by the 'Wave' projectile.
    The shield is 'Burnable' using the Fireball weapon, which will be destroyed after a while.

*/

using UnityEngine;

namespace RL.Entities
{
    /// <summary>
    /// Shield sported by enemies that blocks player projectiles.
    /// </summary>
    public class EnemyShield : MonoBehaviour, IBurnable
    {
        public bool IsBurnable;
        Color prevColor;
        SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            prevColor = spriteRenderer.color;
        }

        public void Burn()
        {
            if (IsBurnable)
            {
                Flash();
            }
        }
        
        public void Flash()
        {
            spriteRenderer.color = prevColor;
            LeanTween.cancel(gameObject);
            spriteRenderer.color = Color.white;
            LeanTween.value(gameObject, Color.white, prevColor, 0.25f)
                .setDelay(1f)
                .setOnUpdate((Color i) =>
                {
                    spriteRenderer.color = i;
                })
                .setEase(LeanTweenType.easeOutSine)
                .setLoopClamp(5)
                .setOnComplete(() =>
                {
                    Destroy(gameObject);
                });
        }
    }
}