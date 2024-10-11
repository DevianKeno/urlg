using System;
using UnityEngine;

namespace RL.Levels
{
    public class EntryTrigger : MonoBehaviour
    {
        [SerializeField] Room room;
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other is BoxCollider2D) return;
            
            if (other.gameObject.CompareTag("Player"))
            {
                if (room.IsActive && !room.IsCleared) return;
                StartCoroutine(room.OnPlayerEntry());
                enabled = false;
            }
        }
    }
}