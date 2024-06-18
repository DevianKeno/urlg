using UnityEngine;

namespace RL.Enemies
{
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