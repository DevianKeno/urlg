using UnityEngine;
using UnityEngine.InputSystem;
using RL;
using TMPro;
using UnityEngine.UI;

namespace RL.UI
{
    public class WeaponsDisplayUI : Window
    {
        public bool EnableSwapping;
        public float SelectorSpeed = 0.3f;
        public LeanTweenType SelectorEase;
        InputAction swapInput;
        
        [SerializeField] GameObject selector;
        [SerializeField] GameObject Selector
        {
            get
            {
                if (selector == null)
                {
                    selector = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Selector"), transform);
                    var le = selector.AddComponent<LayoutElement>();
                    le.ignoreLayout = true;
                    selector.transform.SetAsLastSibling();
                }
                return selector;
            }
        }
        public WeaponIconUI weapon1;
        public WeaponIconUI weapon2;
        [SerializeField] TextMeshProUGUI numberTmp;

        void Start()
        {
            var map = Game.Main.PlayerInput.actions.FindActionMap("Player");
            swapInput = map.FindAction("Cheats");
            
            swapInput.started += OnInputSwap;
        }

        void OnInputSwap(InputAction.CallbackContext context)
        {
            if (!EnableSwapping) return;
            if (!int.TryParse(context.control.displayName, out int index)) return;

            Transform target;
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

            LeanTween.move(Selector, target.position, SelectorSpeed)
                .setEase(SelectorEase);
            LeanTween.scale(Selector, target.localScale, SelectorSpeed)
                .setEase(SelectorEase);
        }
    }
}