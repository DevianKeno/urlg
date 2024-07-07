using System;

using UnityEngine;

namespace RL
{
    [Serializable]
    [CreateAssetMenu(fileName = "Particle", menuName = "Particle")]
    public class ParticleData : ScriptableObject
    {
        public string Id;
        public GameObject Prefab;
    }
}