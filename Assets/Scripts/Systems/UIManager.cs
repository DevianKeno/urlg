using UnityEngine;
using UnityEngine.UI;

namespace URLG
{
    public class UIManager : MonoBehaviour
    {
        public Image vignette;

        public void VignetteDamageFlash()
        {
            LeanTween.value(gameObject, 0.5f, 0, 1f)
            .setOnUpdate((float i) =>
            {
                vignette.color = new(0.5f, 0f, 0f, i); /// red
            })
            .setEase(LeanTweenType.easeOutSine);
        }
    }
}