using URLG.Levels;
using UnityEngine;

namespace URLG.Player
{
    public class PlayerLoadingTrigger : MonoBehaviour
    {
        [SerializeField] Collider2D trigger;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == null) return;
            
            if (other.CompareTag("Room"))
            {
                if (other.gameObject.TryGetComponent<ILoadable>(out var content))
                {
                    content.Load();
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject == null) return;

            if (other.CompareTag("Room"))
            {
                if (other.gameObject.TryGetComponent<ILoadable>(out var content))
                {
                    content.Unload();
                }
            }
        }
    }
}