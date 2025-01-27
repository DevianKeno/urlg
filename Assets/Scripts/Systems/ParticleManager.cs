/*

Component Title: Particle Manager
Data written: July 7, 2024
Date revised: October 26, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component manages the game's particle system.
    Contains methods necessary for spawning in visual particles.
        e.g., Create()

Data Structures:
    Dictionary: used to store the loaded particles for the game
        Key is the Id of the particle; Value is the particle data itself
*/

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

        /// <summary>
        /// Create a particle given a valid Id.
        /// </summary>
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