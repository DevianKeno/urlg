using UnityEngine;

namespace RL
{
    public class Persistent : MonoBehaviour
    {
        static Persistent _instance;

        void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            } else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }   
        }
    }
}