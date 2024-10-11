using System;
using RL.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RL
{
    public class UIManager : MonoBehaviour
    {
        // public ArrowPointer ArrowPointer;
        public TransitionOptions TransitionOptions = new();

        [SerializeField] Image vignette;
        [SerializeField] Canvas transitionCanvas;

        // void Awake()
        // {
        //     ArrowPointer = GetComponentInChildren<ArrowPointer>();
        // }

        // public void HideArrowPointer()
        // {
        //     ArrowPointer.gameObject.SetActive(false);
        // }

        // public void ShowArrowPointer()
        // {
        //     ArrowPointer.gameObject.SetActive(true);
        // }
        
        TransitionEffect transitionEffect;

        public void PlayTransitionHalf(Action callback = null)
        {
            var go = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Transition"));
            transitionEffect = go.GetComponent<TransitionEffect>();

            transitionEffect.transform.SetParent(transitionCanvas.transform);
            transitionEffect.SetOptions(TransitionOptions);
            transitionEffect.PlayToHalf(callback);
        }

        public void PlayTransitionEnd(Action callback = null)
        {
            transitionEffect.PlayToEnd(callback);
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