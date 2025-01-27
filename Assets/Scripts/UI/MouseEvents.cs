/*

Component Title: Mouse Events
Data written: September 28, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component is attached to UI elements that has is associated with mouse events (e.g., Hover, Click, etc.).
    Other scripts can listen to the provided events, which are fired when the user interacts with the UI element.

Data Structures:
    N/A
*/

using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RL.UI
{
    public class MouseEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Button btn;
        public Button Button => btn;

        public event EventHandler<PointerEventData> OnMouseEnter;
        public event EventHandler<PointerEventData> OnMouseExit;
        public event EventHandler<PointerEventData> OnMouseDown;
        public event EventHandler<PointerEventData> OnMouseUp;

        void Awake()
        {
            btn = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit?.Invoke(this, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMouseDown?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnMouseUp?.Invoke(this, eventData);
        }
    }
}