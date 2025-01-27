/*

Component Title: State Machine
Data written: June 12, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Finite-automata state machine (FSM) implementation, supplied by an Enum
    representing all possible states the machine can transition to.
    This component is attached to all objects that can have 'states' and transition to them.
    For example, an Enemy might have a 'Walk', 'Run', or 'Dead' state, etc.
    Contains methods for switching to a different state, events for assigning callbacks,
    and virtual functions for creating subclasses.

Control:
    The state machine is initialized by setting the states with an Enum that
    contains all possible states this machine can transition to.

Data Structures:
    Dictionary: used to store all possible states this state machine can transition to
        Key is the Enum value of the State; Value is the State data struct itself
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Systems
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
        /// Transition to target state.
        /// Given state must only be of the allowed values set when this machine was initialized.
        /// </summary>
        public virtual void ToState(State<E> state)
        {
            if (_currentState == state) return;            
            TrySwitchState(state);
        }

        /// <summary>
        /// Transition to target state.
        /// Given state must only be of the allowed values set when this machine was initialized.
        /// </summary>
        public virtual void ToState(E state)
        {
            if (!_states.ContainsKey(state)) return;
            ToState(_states[state]);
        }

        /// <summary>
        /// Lock the current state indefinitely.
        /// Warning: this will prevent all suceeding transitions unless Unlock() is called
        /// </summary>
        public virtual void Lock()
        {
            LockedUntil = float.MaxValue;
        }
        
        /// <summary>
        /// Forcibly unlock the current state.
        /// </summary>
        public virtual void Unlock()
        {
            LockedUntil = Time.time;
        }

        /// <summary>
        /// Lock the current state for a specified duration seconds.
        /// </summary>
        public virtual void LockFor(float seconds)
        {
            LockedUntil = Time.time + seconds;
        }

        /// <summary>
        /// Called everytime the machine enters a state.
        /// </summary>
        protected virtual void OnEnterState(E state) { }

        /// <summary>
        /// Called everytime the machine exits a state.
        /// </summary>
        protected virtual void OnExitState(E state) { }

        /// <summary>
        /// Transition to the given state. Given state must only be of the allowed values set when this machine was initialized.
        /// Locking this will prevent the machine from transitioning to other states for the given amount of seconds.
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