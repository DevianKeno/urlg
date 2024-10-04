using System;

using UnityEngine;
using TMPro;

using RL.Telemetry;
using UnityEngine.UI;

namespace RL.UI
{
    [Serializable]
    public class TelemetryEntryUI : MonoBehaviour
    {
        public StatKey Key;
        int value;
        public int Value
        {
            get
            {
                value = int.Parse(inputField.text);
                return value;
            }
            set
            {
                this.value = value;
                inputField.text = this.value.ToString();
            }
        }
        public bool IsIncludedInRandom { get; set; }
        
        public Button includeInRandomBtn;
        public TMP_InputField inputField;
        
        void Awake()
        {
            includeInRandomBtn?.onClick.AddListener(IncludeInRandom);
            inputField = GetComponent<TMP_InputField>();
        }

        void IncludeInRandom()
        {
            
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                inputField ??= GetComponent<TMP_InputField>();
            }
        }
    }
}