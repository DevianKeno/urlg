using System;

using UnityEngine;

namespace URLG
{
    [Serializable]
    [CreateAssetMenu(fileName = "Particle", menuName = "Particle")]
    public class ParticleData : ScriptableObject
    {
        public string Id;
        public GameObject Prefab;
    }
}