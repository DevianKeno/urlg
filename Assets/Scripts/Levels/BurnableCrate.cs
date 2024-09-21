using System;
using TMPro;
using UnityEngine;

namespace URLG.Levels
{
    public class BurnableCrate : Tile
    {
        public float Health = 100f;
        public float BurnTime = 3f;

        [SerializeField] SpriteRenderer spriteRendererChild;

        public void StartBurning()
        {
            var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
            Instantiate(flamePrefab, transform);
            DamageFlash();
            Destroy(gameObject, BurnTime);
        }

        public void TakeDamage(float amount)
        {
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

        public void Break()
        {
            Destroy(gameObject);
        }
    }
}