/*

Component Title: Frame Controller

Data written: September 30, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Where the program fits in the general system design:
    The frame controller is attached to screens that shall display multiple frames. It is quite similar to a card view.

Purpose:
    UI handler for displaying a specific frame from a subset of frames.
    It enables switching between frames using smooth animations or instant transitions.
    This component also provides mechanisms for dynamic frame addition and removal.

Control:
    First initialize included frames then transition using the SwitchToFrame(name) methods.
    These method accepts the frame name, index, as well as additional arguments for animation behavior.

Data Structures/Key Variables:
    - SwitchStatus
    - FrameController.SwitchFrameContext
    [Definitions are found at their respective declarations]
*/

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class FrameController : MonoBehaviour
    {
        public enum SwitchStatus {
            Started, Finished
        }

        /// <summary>
        /// Data structure to store information for the <c>OnSwitchFrame</c> event.
        /// </summary>
        public struct SwitchFrameContext
        {
            /// <summary>
            /// The point in time the <c>OnSwitchFrame</c> event is at.
            /// </summary>
            public SwitchStatus Status { get; set; }
            /// <summary>
            /// The previous frame, or the frame that was transitioned from.
            /// </summary>
            public string Previous { get; set; }
            /// <summary>
            /// The next frame, or the frame that is being transitioned to.
            /// </summary>
            public string Next { get; set; }
        }

        /// <summary>
        /// The size of the frame. [Set in Inspector]
        /// </summary>
        public Vector2 FrameSize;
        /// <summary>
        /// The position of the frame when it is in inactive state. [Set in Inspector]
        /// </summary>
        public Vector2 InactiveFramePosition;
        /// <summary>
        /// Lerp duration. [Set in Inspector]
        /// </summary>
        public float AnimationFactor = 0.5f;
        /// <summary>
        /// Tween animation type. [Set in Inspector]
        /// </summary>
        public LeanTweenType Ease = LeanTweenType.easeOutExpo;
        public bool IsTransitioning { get; private set; }
        [SerializeField] Frame currentFrame;
        /// <summary>
        /// The currently displayed frame.
        /// </summary>
        public Frame CurrentFrame => currentFrame;
    
        string _previousFrame;
        [SerializeField] List<Frame> frames = new();

        /// <summary>
        /// Called everytime this controller switches frames.
        /// </summary>
        public event Action<SwitchFrameContext> OnSwitchFrame;


#region Editor
        [SerializeField] string switchTo;
#endregion


        void Start()
        {
            if (currentFrame == null && transform.childCount > 0)
            {
                currentFrame = transform.GetChild(0).GetComponent<Frame>();
            }
        }

        /// <summary>
        /// Switch to frame given its index.
        /// </summary>
        public void SwitchToFrame(int index)
        {
            SwitchToFrame(frames[index].Name, instant: false, force: false);
        }

        /// <summary>
        /// Switch to frame given the name.
        /// </summary>
        public void SwitchToFrame(string name)
        {
            SwitchToFrame(name, instant: false, force: false);
        }

        /// <summary>
        /// Switch to frame given the name.
        /// </summary>
        public void SwitchToFrame(string name, bool instant = false)
        {
            SwitchToFrame(name, instant, force: false);
        }

        /// <summary>
        /// Switch to frame given the name.
        /// </summary>
        /// <param name="name">Name of the frame to switch to</param>
        /// <param name="instant">If <c>false</c>, plays a tween animation</param>
        /// <param name="force">By default, this cannot transition if there is an animation playing</param>
        public void SwitchToFrame(string name, bool instant = false, bool force = false)
        {
            Frame frame = GetFrame(name);
            if (frame == null) return;

            if (IsTransitioning) return;
            IsTransitioning = true;

            _previousFrame = currentFrame.Name;
            var context = new SwitchFrameContext()
            {
                Status = SwitchStatus.Started,
                Previous = _previousFrame,
                Next = name,
            };
            OnSwitchFrame?.Invoke(context);

            // if (Game.UI.EnableScreenAnimations && !instant)
            if (!instant)
            {
                if (currentFrame != null && currentFrame.Name != frame.Name)
                {
                    /// Move current frame out of the way
                    LeanTween.move(currentFrame.Rect, new Vector2(-1920, 0f), AnimationFactor)
                    .setEase(Ease)
                    .setOnComplete(() =>
                    {
                        currentFrame.Rect.anchoredPosition = InactiveFramePosition;
                    });
                }
                else if (!force)
                {
                    IsTransitioning = false;
                    return;
                }

                /// Move new frame into view
                LeanTween.move(frame.Rect, Vector2.zero, AnimationFactor)
                .setEase(Ease)
                .setOnComplete((() =>
                {
                    currentFrame = frame;
                    currentFrame.transform.SetAsLastSibling();
                    IsTransitioning = false;

                    context.Status = SwitchStatus.Finished;
                    OnSwitchFrame?.Invoke(context);
                }));
                LayoutRebuilder.ForceRebuildLayoutImmediate(frame.transform as RectTransform);
            }
            else
            {
                IsTransitioning = true;
                currentFrame.Rect.anchoredPosition = new Vector2(-1920, 0f); /// Hide current frame
                frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
                currentFrame = frame;
                currentFrame.transform.SetAsLastSibling();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
                IsTransitioning = false;
                context.Status = SwitchStatus.Finished;
                OnSwitchFrame?.Invoke(context);
            }
        }

        /// <summary>
        /// Appends an external frame to this controller.
        /// </summary>
        public void AppendFrame(Frame frame, bool display = true)
        {
            frame.transform.SetParent(transform);
            frame.Rect.sizeDelta = Vector2.zero;
            frames.Add(frame);
            if (display)
            {
                SwitchToFrame(frame.Name, instant: true);
            }
            else
            {
                frame.Rect.anchoredPosition = InactiveFramePosition;
            }
        }

        /// <summary>
        /// Removes a frame from this controller.
        /// </summary>
        public void RemoveFrame(Frame frame)
        {
            if (frames.Contains(frame))
            {
                frames.Remove(frame);
            }
            SwitchToFrame(_previousFrame, instant: true);
        }

        /// <summary>
        /// Retrieves a frame from this controller given its name.
        /// </summary>
        public Frame GetFrame(string name)
        {
            foreach (Frame f in frames)
            {
                if (f.Name != name) continue;
                return f;
            }
            return null;
        }


        #region Editor

        public void SwitchFrameEditor(string frameId)
        {
            switchTo = frameId;
            SwitchFrameEditor();
        }

        [ContextMenu("Switch Frames")]
        public void SwitchFrameEditor()
        {
            Frame frame = null;
            if (int.TryParse(switchTo, out int index))
            {
                if (index >= 0 && index < frames.Count)
                {
                    frame = frames[index];
                }
            }
            else
            {
                frame = GetFrame(switchTo);
            }
            if (frame == null) return;
            
            currentFrame.Rect.anchoredPosition = InactiveFramePosition;
            frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
            currentFrame = frame;
            currentFrame.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        #endregion
    }
}