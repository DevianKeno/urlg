using RL.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RL
{
    public class UIManager : MonoBehaviour
    {
        public ArrowPointer ArrowPointer;
        public Image vignette;

        void Awake()
        {
            ArrowPointer = GetComponentInChildren<ArrowPointer>();
        }

        public void HideArrowPointer()
        {
            ArrowPointer.gameObject.SetActive(false);
        }

        public void ShowArrowPointer()
        {
            ArrowPointer.gameObject.SetActive(true);
        }

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