/*

Component Title: Window
Data written: October 4, 2024
Date revised: October 14, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    General purpose window UI element.

Data Structures:
    Vector2: used to store the position of this UI element in the screen
*/

using System;

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class Window : MonoBehaviour
    {
        [SerializeField] protected RectTransform rect;
        public RectTransform Rect => rect;
        [field: Space]

        public bool IsVisible { get; set; }
        public Vector2 Position
        {
            get { return rect.anchoredPosition; }
            set { rect.anchoredPosition = value; }
        }
        public Vector2 Pivot
        {
            get { return rect.pivot; }
            set { rect.pivot = value; }
        }
        public float FadeDuration = 0.3f;
        
        protected GameObject blocker;


        #region Events

        /// <summary>
        /// Called whenever this Window is shown/opened.
        /// </summary>
        public event Action OnOpen;
        /// <summary>
        /// Called whenever this Window is hidden/closed.
        /// </summary>
        public event Action OnClose;

        #endregion
        

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        void Start()
        {
            Show();
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                rect ??= GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Executes once ONLY IF the window is previously hidden, then made visible.
        /// </summary>
        public virtual void OnShow() { }
        /// <summary>
        /// Executes once ONLY IF the window is previously visible, then made hidden.
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (!IsVisible)
            {
                OnShow();
                OnOpen?.Invoke();
            }
            
            IsVisible = true;
        }

        /// <summary>
        /// Hides the window.
        /// Does not destroy nor disables the object.
        /// Use Destroy() if you need to delete, and SetActive() to disable.
        /// </summary>
        public void Hide(bool destroy = false, float delay = 0)
        {
            if (IsVisible)
            {
                OnClose?.Invoke();
                OnHide();
            }

            gameObject.SetActive(false);
            IsVisible = false;
            if (destroy)
            {
                Destroy(gameObject, delay);
            }
        }

        public void SetActive(bool enabled)
        {
            if (enabled)
            {
                gameObject.SetActive(true);
                Show();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
            else
            {
                Hide();
                // Game.UI.RemoveFromActiveWindows(this);
                gameObject.SetActive(false);
            }
        }

        public void SetParent(Transform p)
        {
            transform.SetParent(p);
        }

        // public void FadeIn()
        // {

        // }

        // public void FadeOut()
        // {

        // }

        public void ToggleVisibility()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
            IsVisible = visible;
        }

        public void SetScale(float multiplier)
        {
            Vector2 dimensions = rect.localScale;
            dimensions.x *= multiplier; 
            dimensions.y *= multiplier; 
            rect.localScale = dimensions;
        }

        public void SetScale(float width, float height)
        {
            rect.localScale = new(width, height);
        }
        
        public void Move(Vector3 position)
        {
            rect.position = position;
        }

        public void Rebuild()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}
