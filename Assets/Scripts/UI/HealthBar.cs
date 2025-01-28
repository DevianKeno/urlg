/*

Component Title: Health Bar
Data written: October 7, 2024
Date revised: November 8, 2024

Programmer/s:
    John Franky Nathaniel V. Batisla-Ong

Purpose:


Data Structures/Key Variables:
    [Defined below]
*/

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class HealthBar : Window
    {
        /// <summary>
        /// UNDEFINED
        /// </summary>
        public float MaximumHealth;
        float actualHealth;
        /// <summary>
        /// UNDEFINED
        /// </summary>
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