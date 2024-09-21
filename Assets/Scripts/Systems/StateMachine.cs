using System;
using System.Collections.Generic;
using UnityEngine;

namespace URLG.Systems
{
    /// <summary>
    /// Base class for State Machines.
    /// </summary>
    public abstract class StateMachine<E> : MonoBehaviour where E : Enum
    {
        public struct StateChanged
        {
            public E From;
            public E To;
            /// <summary>
            /// The state to change to.
            /// </summary>
            public E State => To;
        }
        
        Dictionary<E, State<E>> _states = new();
        public Dictionary<E, State<E>> States => _states;
        public State<E> InitialState;
        [SerializeField] State<E> _currentState;
        public E CurrentState => _currentState.Key;
        public float LockedUntil { get; private set; }
        public bool IsTransitioning { get; private set; }
        
        public E InState;
        public bool DebugMode = false;
        
        /// <summary>
        /// Called everytime before the State changes.
        /// </summary>
        public event EventHandler<StateChanged> OnStateChanged;

        void Awake()
        {
            foreach (E state in Enum.GetValues(typeof(E)))
            {
                _states[state] = new State<E>(state);
            }
        }

        void Start()
        {
            if (InitialState != null) _currentState = InitialState;
        }

        /// <summary>
        /// Transition to state.
        /// </summary>
        public virtual void ToState(State<E> state)
        {
            if (_currentState == state) return;            
            TrySwitchState(state);
        }

        public virtual void ToState(E state)
        {
            if (!_states.ContainsKey(state)) return;
            ToState(_states[state]);
        }

        public virtual void Lock()
        {
            LockedUntil = float.MaxValue;
        }
        
        public virtual void Unlock()
        {
            LockedUntil = Time.time;
        }

        public virtual void LockFor(float seconds)
        {
            LockedUntil = Time.time + seconds;
        }

        protected virtual void OnEnterState(E state)
        {

        }

        protected virtual void OnExitState(E state)
        {
            
        }

        /// <summary>
        /// Transition to state and prevent from transitioning to other states for a certain amount of seconds.
        /// </summary>
        public virtual void ToState(State<E> state, float lockForSeconds)
        {
            if (_currentState == state) return;
            TrySwitchState(state, lockForSeconds);
        }

        bool TrySwitchState(State<E> state, float lockForSeconds = 0f)
        {
            if (Time.time < LockedUntil) return false;
            IsTransitioning = true;
            LockedUntil = Time.time + lockForSeconds;

            var previousState = _currentState;
            _currentState.Exit();
            _currentState = state;
            InState = state.Key;
            _currentState.Enter();
            IsTransitioning = false;

            OnStateChanged?.Invoke(this, new()
            {
                From = previousState.Key,
                To = state.Key
            });
            if (DebugMode) Debug.Log("Switched to state " + state);
            return true;
        }
    }
}