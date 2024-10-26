using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class HoldButton : MonoBehaviour
    {
        [Range(0, 1), SerializeField] float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                fillImage.fillAmount = progress;
            }
        }

        [SerializeField] Image fillImage;
        public Image FillImage => fillImage;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                fillImage.fillAmount = progress;
            }
        }
    }
}