using UnityEngine;

namespace RL
{
    public class Particle : MonoBehaviour
    {
        public ParticleData Data;

        void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}