/*

Component Title: Telemetry Entry User Interface
Data written: October 4, 2024
Date revised: October 14, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Represents a single UI element instance of a stat, displayed along others in the Telemetry GUI.

Data Structures:
    StatKey: the actual stat to represent
*/

using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using RL.Telemetry;

namespace RL.UI
{
    [Serializable]
    public class TelemetryEntryUI : MonoBehaviour
    {
        /// <summary>
        /// The actual stat to represent.
        /// </summary>
        public StatKey Key;
        int value;
        /// <summary>
        /// The runtime value of the stat represented.
        /// </summary>
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
        /// <summary>
        /// Whether to include this stat when generating random features.
        /// </summary>
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
            /// irrelevant
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