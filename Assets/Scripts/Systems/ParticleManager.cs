using System.Collections.Generic;

using UnityEngine;

namespace RL
{
    public class ParticleManager : MonoBehaviour
    {
        Dictionary<string, Particle> _particlesDict = new();
        
        public void Initialize()
        {
            var gos = Resources.LoadAll<GameObject>("Prefabs/Particles");

            foreach (GameObject go in gos)
            {
                if (go.TryGetComponent<Particle>(out var p))
                {
                    _particlesDict[p.Data.Id] = p;
                }
            }
        }
        public Particle Create(string id)
        {
            if (_particlesDict.ContainsKey(id))
            {
                var go = Instantiate(_particlesDict[id].Data.Prefab, transform);
                return go.GetComponent<Particle>();
            }
            return null;
        }
    }
}