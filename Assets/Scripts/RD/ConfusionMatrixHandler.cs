using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RL.RD.UI
{
    public enum ConfusionMatrixValue {
        None, TruePositive, TrueNegative, FalsePositive, FalseNegative,
    }

    public class ConfusionMatrixHandler : MonoBehaviour
    {
        public ConfusionMatrixValue CurrentValue = ConfusionMatrixValue.None;

        public Color ActiveColor;
        public Color InactiveColor;

        [SerializeField] TextMeshProUGUI tpTmp;
        [SerializeField] TextMeshProUGUI tnTmp;
        [SerializeField] TextMeshProUGUI fpTmp;
        [SerializeField] TextMeshProUGUI fnTmp;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                SetValue(CurrentValue);
            }
        }

        public void SetValue(ConfusionMatrixValue value)
        {
            CurrentValue = value;
            tpTmp.color = InactiveColor;
            tnTmp.color = InactiveColor;
            fpTmp.color = InactiveColor;
            fnTmp.color = InactiveColor;

            switch (value)
            {
                case ConfusionMatrixValue.TruePositive:
                {
                    tpTmp.color = ActiveColor;
                    break;
                }
                case ConfusionMatrixValue.TrueNegative:
                {
                    tnTmp.color = ActiveColor;
                    break;
                }
                case ConfusionMatrixValue.FalsePositive:
                {
                    fpTmp.color = ActiveColor;
                    break;
                }
                case ConfusionMatrixValue.FalseNegative:
                {
                    fnTmp.color = ActiveColor;
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
        
    }
}