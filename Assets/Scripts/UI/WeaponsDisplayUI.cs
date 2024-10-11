using UnityEngine;
using UnityEngine.InputSystem;
using RL;
using TMPro;

namespace RL.UI
{
    public class WeaponsDisplayUI : MonoBehaviour
    {
        public float SelectorSpeed = 0.3f;
        public LeanTweenType SelectorEase;
        InputAction swapInput;
        
        [SerializeField] Transform selector;
        [SerializeField] WeaponIconUI weapon1;
        [SerializeField] WeaponIconUI weapon2;
        [SerializeField] TextMeshProUGUI numberTmp;

        void Start()
        {
            var map = Game.Main.PlayerInput.actions.FindActionMap("Player");
            swapInput = map.FindAction("Cheats");
            
            swapInput.started += OnInputSwap;
        }

        void OnInputSwap(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            Transform target = null;
            LeanTween.cancel(selector.gameObject);

            if (index == 1)
            {
                target = weapon1.transform;
            }
            else if (index == 2)
            {
                target = weapon2.transform;
            }
            else
            {
                return;
            }

            LeanTween.move(selector.gameObject, target.position, SelectorSpeed)
                .setEase(SelectorEase);
            LeanTween.scale(selector.gameObject, target.localScale, SelectorSpeed)
                .setEase(SelectorEase);
        }
    }
}