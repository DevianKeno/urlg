using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace RL.Levels
{
    public class BurnableCrate : Tile
    {
        public float Health = 200f;
        public float BurnTime = 3f;

        OnFire onFire;

        bool _isBurning = false;

        [SerializeField] SpriteRenderer spriteRendererChild;

        public void StartBurning(float duration)
        {
            if (_isBurning) return;
            _isBurning = true;
            
            onFire = gameObject.AddComponent<OnFire>();
            onFire.OnTick += OnFireTick;
            onFire.StartBurn(duration);
            // var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
            // Instantiate(flamePrefab, transform);
            // DamageFlash();
            // Break(BurnTime);
        }

        void OnFireTick()
        {
            TakeDamage(Game.BurnDamage);
        }

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

        public void TakeDamageSilent(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Break();
            }
        }

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