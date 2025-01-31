/*

Program Title: 
Date written: June 22, 2024
Date revised: November 8, 2024

Programmer/s:
    John Paulo A. Dela Cruz

Purpose:
    This script defines the Burnable Crates component,that can take damage and burn over time.
    The crate interacts with fire mechanics and has visual and audio effects for damage and destruction.

Control:


Data Structures/Key Variables:
    Health (float) – Stores the crate’s current health, starting at 200f.
    BurnTime (float) – Defines how long the crate will burn before the fire stops.
    IsBurning (bool) – A flag that determines whether the crate is currently burning.
    Coroutine (IEnumerator BreakCoroutine) – Handles delayed destruction of the crate when broken.

*/

using System;
using System.Collections;

using UnityEngine;
using TMPro;

namespace RL.Levels
{
    public class BurnableCrate : Tile
    {
        public float Health = 200f; // The total health of the crate
        public float BurnTime = 3f; // The duration for which the crate burns

        public bool IsBurning => _isBurning; // Read-only property to check if the crate is burning
        bool _isBurning = false;

        OnFire onFire;
        [SerializeField] SpriteRenderer spriteRendererChild;

        /// <summary>
        /// Starts burning the crate if it's not already burning.
        /// </summary>
        public void StartBurning(float duration)
        {
            if (_isBurning) return;
            _isBurning = true;
            
            onFire = gameObject.AddComponent<OnFire>();
            onFire.OnTick += OnFireTick;
            onFire.StartBurn(duration);
        }
         /// <summary>
        /// Called every fire tick to apply burn damage.
        /// </summary>
        void OnFireTick()
        {
            TakeDamageSilent(Game.BurnDamage);
        }
        /// <summary>
        /// Reduces health and checks if the crate should break.
        /// Plays a hit sound and damage effect.
        /// </summary>
        public void TakeDamage(float amount)
        {
            Game.Audio.Play("crate_hit");
            DamageFlash();

            Health -= amount;
            if (Health <= 0)
            {
                Break();
            }
        }
        /// <summary>
        /// Reduces health silently without sound or visual feedback.
        /// </summary>
        public void TakeDamageSilent(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Break();
            }
        }
        /// <summary>
        /// Creates a red-to-white flash effect when damaged.
        /// </summary>
        public void DamageFlash()
        {
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, Color.red, Color.white , 0.2f)
                .setOnUpdate((Color color) =>
                {
                    spriteRenderer.color = color; 
                    spriteRendererChild.color = color;
                });
        }

        public void Break(float delaySeconds = 0f)
        {
            StartCoroutine(BreakCoroutine(delaySeconds));
        }

        /// <summary>
        /// Coroutine that handles the crate destruction process.
        /// </summary
        IEnumerator BreakCoroutine(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);

            Game.Audio.Play("crate_break");

            if (_isBurning) onFire?.StopBurn();

            coll.enabled = false; 
            spriteRendererChild.enabled = false;
            var animator = GetComponent<Animator>();
            animator.enabled = true;

            yield return new WaitForSeconds(2f);
            
            Destroy(gameObject);
        }
    }
}