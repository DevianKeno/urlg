using System.Collections;
using System.Collections.Generic;
using RL.Projectiles;
using UnityEngine;

namespace RL
{    
    public class AntiWaveArea : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var go = other.gameObject;

            if (go.CompareTag("Wave"))
            {
                var wave = go.GetComponent<Wave>();
                wave.Dissipate();
            }
        }
    }
}