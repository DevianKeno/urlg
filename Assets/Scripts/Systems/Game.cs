using UnityEngine;
using RL.Systems;

namespace RL
{
    public class Game : MonoBehaviour
    {
        static AudioManager audioManager;
        public static AudioManager Audio => audioManager;
        static Telemetry telemetry;
        public static Telemetry Telemetry => telemetry;

        void Awake()
        {
            audioManager = GetComponentInChildren<AudioManager>();
            telemetry = GetComponentInChildren<Telemetry>();
        }

        void Start()
        {
            telemetry.Initialize();
        }
    }
}