using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class HealthBar : MonoBehaviour
    {
        public Slider bufferSlider;
        public Slider healthSlider;
        public float maximumHealth;
        public float actualHealth;
        private float lerpSpeed = 0.5f;

        public void InitializeMaxHealth(float maximumHealth)
        {
            actualHealth = maximumHealth;
            healthSlider.value = actualHealth;
        }

        public void UpdateHealthPoints(float currentHealth)
        {
            actualHealth = currentHealth;

            if(healthSlider.value != actualHealth)
            {
                healthSlider.value = actualHealth;
            }

            if(healthSlider.value != bufferSlider.value)
            {
                bufferSlider.value = Mathf.Lerp(bufferSlider.value, healthSlider.value, lerpSpeed);
            }
        }
    }
}