/*

Component Title: State Animator
Data written: October 4, 2024
Date revised: June 12, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:


Data Structures:
    N/A
*/
using System;
using UnityEngine;

namespace RL.Systems
{
    [RequireComponent(typeof(Animator))]
    public abstract class StateAnimator<T> : MonoBehaviour where T : Enum
    {
        protected Animator animator;
        protected RuntimeAnimatorController controller;

        void Awake()
        {
            animator = GetComponent<Animator>();
            controller = animator.runtimeAnimatorController;
        }

        /// <summary>
        /// Play an animation given an id.
        /// </summary>
        public void PlayAnim(string id)
        {
            animator.Play(id);
        }

        /// <summary>
        /// Callback for the state changed event.
        /// </summary>
        public void StateChangedCallback(object sender, StateMachine<T>.StateChanged e)
        {
            var id = GetAnimIdFromState(e.State);
            animator.Play(id);
        }

        /// <summary>
        /// Gets a valid AnimId string from the given state.
        /// </summary>
        public string GetAnimIdFromState(T state)
        {
            return state.ToString().ToLower();
        }
    }
}