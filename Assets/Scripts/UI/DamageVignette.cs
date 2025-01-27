/*

Component Title: Damage Vignette
Data written: October 12, 2024
Date revised: October 17, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Houses the vignette texture that occupies the edges of the screen.
    This vignette flashes to red, then slowly fades away
    whenever the player takes damage from enemies.

Data Structures/Key Variables:
    N/A
*/

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class DamageVignette : Window
    {
        [SerializeField] Image image;
        
        public void DamageFlash()
        {
            LeanTween.value(gameObject, 1f, 0, 1f)
            .setOnUpdate((float i) =>
            {
                image.color = new(0.5f, 0f, 0f, i); /// red
            })
            .setEase(LeanTweenType.easeOutSine);
        }
    }
}