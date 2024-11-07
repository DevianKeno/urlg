using System;
using UnityEngine;

namespace RL
{
    public class OnFire : MonoBehaviour
    {
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