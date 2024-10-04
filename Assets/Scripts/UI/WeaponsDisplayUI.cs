using UnityEngine;
using UnityEngine.InputSystem;
using RL;

namespace RL.UI
{
    public class WeaponsDisplayUI : MonoBehaviour
    {
        public float SelectorSpeed = 0.3f;
        public LeanTweenType SelectorEase;
        InputAction swapInput;
        
        [SerializeField] Transform selector;
        [SerializeField] WeaponIconUI fireballIcon;
        [SerializeField] WeaponIconUI beamIcon;
        [SerializeField] WeaponIconUI waveIcon;

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
                target = fireballIcon.transform;
            }
            else if (index == 2)
            {
                target = beamIcon.transform;
            }
            else if (index == 3)
            {
                target = waveIcon.transform;
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