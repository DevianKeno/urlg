/*

Component Title: Mouse Events

Data written: September 28, 2024
Date revised: October 4, 2024

Programmer/s:
    Edrick L. De Villa

Where the program fits in the general system design:
    This component is attached to UI elements that has is associated with mouse events (e.g., Hover, Click, etc.).

Purpose:
    To provide a handle for mouse interactions, allowing other scripts to listen to the provided events,
    which are fired when the user interacts with the UI element.
    It ensures that any UI element equipped with this component can trigger custom logic by 
    centralizing event handling and making it easier for other scripts to respond to user interactions.
    
Control:
    This component initializes automatically when attached to a GameObject with a UI Button component.
    Other scripts can then register as listeners on its events (`OnMouseEnter`, `OnMouseExit`, 
      `OnMouseDown`, `OnMouseUp`) to execute custom logic.

Data Structures:
    Button: reference to the Button component
    event: callbacks for other methods to listen to
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