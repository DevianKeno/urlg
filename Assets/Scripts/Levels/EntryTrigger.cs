using UnityEngine;

using RL.Player;

namespace RL.Levels
{
    /// <summary>
    /// Event trigger whenever the player passes through.
    /// </summary>
    public class EntryTrigger : MonoBehaviour
    {
        [SerializeField] Room room;
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other is BoxCollider2D) return;
            
            if (other.gameObject.CompareTag("Player"))
            {
                if (room.IsActive && !room.IsCleared) return;
                StartCoroutine(room.OnPlayerEntry(other.GetComponent<PlayerController>()));
                enabled = false;
            }
        }
    }
}