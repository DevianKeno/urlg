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

        [SerializeField] SpriteRenderer spriteRendererChild;

        public void StartBurning()
        {
            var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
            Instantiate(flamePrefab, transform);
            DamageFlash();
            Break(BurnTime);
        }

        public void TakeDamage(float amount)
        {
            Game.Audio.PlaySound("crate_hit");
            Health -= amount;
            DamageFlash();
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

            Game.Audio.PlaySound("crate_break");

            coll.enabled = false; 
            spriteRendererChild.enabled = false;
            var animator = GetComponent<Animator>();
            animator.enabled = true;

            yield return new WaitForSeconds(2f);
            
            Destroy(gameObject);
        }
    }
}