using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using RL.CellularAutomata;
using RL.Systems;
using RL.Levels;
using RL.Player;

namespace RL
{
    public enum PCGAlgorithm { AcceptReject, GaussianNaiveBayes }
    
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
        static EntityManager entityManager;
        public static EntityManager Entity => entityManager;
        static ParticleManager particlesManager;
        public static ParticleManager Particles => particlesManager;

        public event Action OnLateInit;

        public int currentLevel = 1;
        public Room CurrentRoom;
        public PlayerController Player = null;
        
        [SerializeField] PCGAlgorithm algorithmUsed = PCGAlgorithm.AcceptReject; 
        public PCGAlgorithm AlgorithmUsed => algorithmUsed;

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
            UIManager = GetComponentInChildren<UIManager>();
            tilesManager = GetComponentInChildren<TilesManager>();
            entityManager = GetComponentInChildren<EntityManager>();
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
            entityManager.Initialize();
            particlesManager.Initialize();

            OnLateInit?.Invoke();
        }

        public void OpenSettingsMenu()
        {
            
        }

        
        #region Scene management 

        public class LoadSceneOptions
        {
            public string SceneToLoad { get; set; }
            public LoadSceneMode Mode { get; set; }
            public bool ActivateOnLoad { get; set; } = true;
            public float DelaySeconds { get; set; }
            public bool PlayTransition { get; set; }
        }

        event Action onLoadSceneCompleted;
        public void LoadScene(LoadSceneOptions options, Action onLoadSceneCompleted = null)
        {
            try
            {
                StartCoroutine(LoadSceneCoroutine(options, onLoadSceneCompleted));
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Scene does not exist" + e);
            }
        }

        public void UnloadScene(string name, Action onLoadSceneCompleted = null)
        {
            try
            {
                SceneManager.UnloadSceneAsync(name);
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        IEnumerator LoadSceneCoroutine(LoadSceneOptions options, Action onLoadSceneCompleted)
        {
            this.onLoadSceneCompleted += onLoadSceneCompleted;

            var asyncOp = SceneManager.LoadSceneAsync(options.SceneToLoad, options.Mode);
            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }
            asyncOp.allowSceneActivation = true;

            this.onLoadSceneCompleted?.Invoke();
            this.onLoadSceneCompleted = null;
        }

        #endregion


        public void SetAlgorithmAR()
        {
            algorithmUsed = PCGAlgorithm.AcceptReject;
        }

        public void SetAlgorithmGNB()
        {
            algorithmUsed = PCGAlgorithm.GaussianNaiveBayes;
        }
    }
}