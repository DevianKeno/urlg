using UnityEngine;
using RL.Systems;

namespace RL
{
    public class Game : MonoBehaviour
    {
        static AudioManager audioManager;
        public static AudioManager Audio => audioManager;

        void Awake()
        {
            audioManager = GetComponentInChildren<AudioManager>();
        }
    }
}