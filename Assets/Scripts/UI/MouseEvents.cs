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