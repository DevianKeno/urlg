/*

Component Title: State (Base)
Data written: June 12, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Base state representation for state machines.

Data Structures:
    State.ChangedContext: used to store information upon transition to/from states

*/

using System;
using UnityEngine;

namespace RL.Systems
{
    /// <summary>
    /// Base state representation.
    /// </summary>
    [Serializable]
    public class State<EState> where EState : Enum
    {
        /// <summary>
        /// Data structure to store information upon transition to/from states.
        /// </summary>
        public struct ChangedContext
        {
            public EState PreviousState;
            public EState NextState;
        }
        
        EState _key;
        /// <summary>
        /// The representation of this state in Enum type
        /// </summary>
        public EState Key => _key;
        bool _isLocked;
        /// <summary>
        /// Whether to allow transitioning to other states if in this state.
        /// </summary>
        public bool IsLocked => _isLocked;

        [field: Header("Events")]       
        /// <summary>
        /// Called once when entering this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnEnter;
        /// <summary>
        /// Called every game tick when in this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnTick;
        /// <summary>
        /// Called once when exiting this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnExit;

        public State(EState key)
        {
            _key = key;
        }

        public State()
        {
        }

        public void Lock(bool value)
        {
            _isLocked = value;
        }

        internal virtual void Enter()
        {
            OnEnter?.Invoke(this, new()
            {

            });
        }

        internal virtual void Exit()
        {
            OnExit?.Invoke(this, new()
            {
            });
        }
    }
}