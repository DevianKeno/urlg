using System;
using RL.Player;
using UnityEngine;

namespace RL.Levels
{
    public class OutsideTrigger : MonoBehaviour
    {
        [SerializeField] Room room;
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other is BoxCollider2D) return;
            
            if (other.gameObject.CompareTag("Player"))
            {
                if (!room.IsActive) return;
                
                /// Room is active and has doors and if for some reason the player moves out of bounds, put them in the center
                other.transform.position = room.Center.position;
            }
        }
    }
}