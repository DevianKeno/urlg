using System;
using UnityEngine;

namespace URLG.Systems
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

        public void PlayAnim(string id)
        {
            animator.Play(id);
        }

        public void StateChangedCallback(object sender, StateMachine<T>.StateChanged e)
        {
            var id = GetAnimIdFromState(e.State);
            animator.Play(id);
        }

        public string GetAnimIdFromState(T state)
        {
            return state.ToString().ToLower();
        }
    }
}