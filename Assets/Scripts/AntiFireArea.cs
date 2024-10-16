using System.Collections;
using System.Collections.Generic;
using RL.Projectiles;
using UnityEngine;

namespace RL
{    
    public class AntiFireArea : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var go = other.gameObject;

            if (go.CompareTag("Fireball"))
            {
                var fire = go.GetComponent<Fireball>();
                fire.Dissipate();
            }
        }
    }
}