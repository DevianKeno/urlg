using System;
using UnityEngine;
using RL.Systems;

namespace RL
{
    public class Game : MonoBehaviour
    {
        static UIManager UIManager;
        public static UIManager UI => UIManager;
        static AudioManager audioManager;
        public static AudioManager Audio => audioManager;
        static EventsManager eventsManager;
        public static EventsManager Events => eventsManager;
        static Telemetry telemetry;
        public static Telemetry Telemetry => telemetry;
        static TilesManager tilesManager;
        public static TilesManager Tiles => tilesManager;
        static ParticleManager particlesManager;
        public static ParticleManager Particles => particlesManager;

        public event Action OnLateInit;

        [SerializeField] GameObject salamanPrefab;
        [SerializeField] GameObject deerPrefab;
        [SerializeField] GameObject beamWeakPrefab;

        void Awake()
        {
            audioManager = GetComponentInChildren<AudioManager>();
            telemetry = GetComponentInChildren<Telemetry>();
            tilesManager = GetComponentInChildren<TilesManager>();
            particlesManager = GetComponentInChildren<ParticleManager>();
        }

        void Start()
        {
            audioManager.Initialize();
            telemetry.Initialize();
            tilesManager.Initialize();
            particlesManager.Initialize();

            OnLateInit?.Invoke();
        }
    }
}