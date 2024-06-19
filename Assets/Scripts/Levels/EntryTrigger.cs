using System;
using UnityEngine;

namespace RL.Levels
{
    public class EntryTrigger : MonoBehaviour
    {
        [SerializeField] Room room;
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                room?.ShutDoors();
                room?.CountFeatures();
            }
        }
    }
}