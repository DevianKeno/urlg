/*

Component Title: Frame
Data written: September 30, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component is attached to ui

Data Structures/Key Variables:
    Id (string): the unique id representing this frame
    Name (string): the display name of this frame
*/


using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
[RequireComponent(typeof(RectTransform))]
    public class Frame : MonoBehaviour
    {
        public string Id = "frame";
        public string Name = "Frame";
        
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

        public Frame(string name)
        {
            Name = name;
        }

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        }
    }
}