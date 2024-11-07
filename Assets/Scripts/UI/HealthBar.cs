using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class HealthBar : Window
    {
        public float MaximumHealth;
        float actualHealth;
        public float ActualHealth
        {
            get { return actualHealth; }
            set
            {
                actualHealth = value;
                UpdateHealthPoints(value);
            }
        }
        float lerpSpeed = 0.25f;

        [SerializeField] Slider bufferSlider;
        [SerializeField] Slider healthSlider;
        
        public void InitializeMaxHealth(float maximumHealth)
        {
            actualHealth = maximumHealth;
            healthSlider.value = actualHealth;
        }

        public void UpdateHealthPoints(float currentHealth)
        {
            actualHealth = currentHealth;

            if (healthSlider.value != actualHealth)
            {
                healthSlider.value = actualHealth;
            }

            if (healthSlider.value != bufferSlider.value)
            {
                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, bufferSlider.value, healthSlider.value, lerpSpeed)
                    .setOnUpdate((float i) =>
                    {
                        bufferSlider.value = i;
                    });
                // bufferSlider.value = Mathf.Lerp(bufferSlider.value, healthSlider.value, lerpSpeed);
            }
        }
    }
}