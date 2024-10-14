using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class DamageVignette : Window
    {
        [SerializeField] Image image;
        
        public void DamageFlash()
        {
            LeanTween.value(gameObject, 0.5f, 0, 1f)
            .setOnUpdate((float i) =>
            {
                image.color = new(0.5f, 0f, 0f, i); /// red
            })
            .setEase(LeanTweenType.easeOutSine);
        }
    }
}