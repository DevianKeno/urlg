using System;
using UnityEngine;
using UnityEngine.InputSystem;
using URLG.CellularAutomata;
using URLG.Systems;

namespace URLG
{
    public class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }

        static UIManager UIManager;
        public static UIManager UI => UIManager;
        static AudioManager audioManager;
        public static AudioManager Audio => audioManager;
        // static EventsManager eventsManager;
        // public static EventsManager Events => eventsManager;

        static Telemetry.Telemetry telemetry;
        public static Telemetry.Telemetry Telemetry => telemetry;

        #region Generators

        static CellularAutomataHelper ca;
        public static CellularAutomataHelper CA => ca;
        static Generator.Generator generator;
        public static Generator.Generator Generator => generator;

        #endregion

        static FilesManager files;
        public static FilesManager Files => files;
        static TilesManager tilesManager;
        public static TilesManager Tiles => tilesManager;
        static ParticleManager particlesManager;
        public static ParticleManager Particles => particlesManager;

        public event Action OnLateInit;

        public int currentLevel = 1;
        
        [SerializeField] PlayerInput playerInput;
        public PlayerInput PlayerInput => playerInput;

        [SerializeField] GameObject salamanPrefab;
        [SerializeField] GameObject deerPrefab;
        [SerializeField] GameObject beamWeakPrefab;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Main != null && Main != this)
            {
                Destroy(this);
            }
            else
            {
                Main = this;
            }

            audioManager = GetComponentInChildren<AudioManager>();
            files = GetComponentInChildren<FilesManager>();
            telemetry = GetComponentInChildren<Telemetry.Telemetry>();
            tilesManager = GetComponentInChildren<TilesManager>();
            playerInput = GetComponent<PlayerInput>();
            ca = GetComponentInChildren<CellularAutomataHelper>();
            generator = GetComponentInChildren<Generator.Generator>();
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