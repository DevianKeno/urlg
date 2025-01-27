/*

Program Title: 
Date written: November 8, 2024
Date revised: November 8, 2024

Programmer/s:
    John Paulo A. Dela Cruz

Purpose:



Control:


Data Structures/Key Variables:
    
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

        public void StartBurn(float duration)
        {
            IsBurning = true;
            _durationTimer = duration;

            if (fireParticle == null)
            {
                var flamePrefab = Resources.Load<GameObject>("Prefabs/Flame");
                fireParticle = Instantiate(flamePrefab, transform);
            }
        }

        public void StopBurn(bool destroyParticle = true)
        {
            IsBurning = false;
            _durationTimer = 0f;
            if (destroyParticle && fireParticle != null)
            {
                Destroy(fireParticle.gameObject);
            }
        }
        

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