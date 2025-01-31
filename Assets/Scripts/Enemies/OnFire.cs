/*

Component Title: On Fire (Component)

Date written: November 8, 2024
Date revised: November 8, 2024

Programmer/s:
    John Paulo A. Dela Cruz

Where the program fits in the general system design:
    Part of the testbed platform (or Game module), status effects.

Purpose:
    This script defines the OnFire component, which is dynamically added to entities
    that are affected by a burning status. It manages the duration, visual effects, 
    and periodic damage (or other effects) applied to the entity.
    The component also provides methods to start and stop the burning effect, including triggering a 
    callback function for custom tick-based behavior.

Control:
    This script is attached to the entities themselves as soon as they make contact with a burning projectile.

Data Structures/Key Variables:
    IsBurning (bool): whether if the entity is currently burning
    OnTick: an evet fired at regular intervals while burning is in effect
*/

using System;

using UnityEngine;

namespace RL
{
    /// <summary>
    /// This component is dynamically added to entities that are burned
    /// for keeping track of its duration, status, along a callback that is fired every tick. 
    /// </summary>
    public class OnFire : MonoBehaviour
    {
        /// <summary>
        /// Whether if this currently is burning.
        /// </summary>
        public bool IsBurning;

        float _deltaTimer;
        float _durationTimer;
        [SerializeField] GameObject fireParticle;

        public event Action OnTick;
        /// <summary>
        /// Starts the burning effect for a specified duration.
        /// </summary>
        public void StartBurn(float duration)
        {
            IsBurning = true;
            _durationTimer = duration;
        // Ensure the fire particle exists, instantiate if missing
            if (fireParticle == null)
            {
                var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
                fireParticle = Instantiate(flamePrefab, transform);
            }
        }
        /// <summary>
        /// Stops the burning effect and optionally destroys the fire particle.
        /// </summary>
        public void StopBurn(bool destroyParticle = true)
        {
            IsBurning = false;
            _durationTimer = 0f;
            if (destroyParticle && fireParticle != null)
            {
                Destroy(fireParticle.gameObject);
            }
        }
        
        /// <summary>
        /// Updates timers while burning, it calls the OnTick event at set intervals,
        /// and stops burning when duration expires.
        /// </summary>
        void Update()
        {
            if (IsBurning)
            {
                _deltaTimer += Time.deltaTime;

                if (_deltaTimer > Game.FireTickSeconds)
                {
                    OnTick?.Invoke();
                    _deltaTimer = 0f;
                }

                _durationTimer -= Time.deltaTime;
                if (_durationTimer <= 0)
                {
                    StopBurn();
                }
            }
        }
    }
}