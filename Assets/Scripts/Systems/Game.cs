using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using RL.CellularAutomata;
using RL.Systems;
using RL.Levels;
using RL.Player;
using System.Collections.Generic;

namespace RL
{
    public enum PCGAlgorithm { AcceptReject, GaussianNaiveBayes }
    
    public class Game : MonoBehaviour
    {
        public const int MaxEnemiesPerRoom = 12;
        
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

        List<GameObject> currentSceneObjects = new();
        
        [SerializeField] PCGAlgorithm algorithmUsed = PCGAlgorithm.AcceptReject; 
        public PCGAlgorithm AlgorithmUsed => algorithmUsed;

        [SerializeField] PlayerInput playerInput;
        public PlayerInput PlayerInput => playerInput;

        [SerializeField] GameObject salamanPrefab;
        [SerializeField] GameObject deerPrefab;
        [SerializeField] GameObject beamWeakPrefab;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Main != null && Main != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Main = this;

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

                playerInput = GetComponent<PlayerInput>();
            }
        }

        void Start()
        {
            UIManager.Initialize();
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

        Dictionary<string, AsyncOperation> sceneOperations = new();

        public class LoadSceneOptions
        {
            public string SceneToLoad { get; set; }
            public LoadSceneMode Mode { get; set; }
            public bool ActivateOnLoad { get; set; } = true;
            public float DelaySeconds { get; set; }
            public bool PlayTransition { get; set; } = true;
        }

        event Action onLoadSceneCompleted;
        public void LoadScene(LoadSceneOptions options, Action onLoadSceneCompleted = null)
        {
            try
            {
                StartCoroutine(LoadSceneCoroutine(options, onLoadSceneCompleted));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        event Action onUnloadSceneCompleted;
        public void UnloadScene(string name, Action onUnloadSceneCompleted = null)
        {
            try
            {
                StartCoroutine(UnloadSceneCoroutine(name, onUnloadSceneCompleted));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        event Action onActivateSceneCompleted;
        public void ActivateScene(string name, Action onActivateSceneCompleted = null)
        {
            try
            {
                StartCoroutine(ActivateSceneCoroutine(name, onActivateSceneCompleted));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        IEnumerator LoadSceneCoroutine(LoadSceneOptions options, Action onLoadSceneCompleted)
        {
            this.onLoadSceneCompleted += onLoadSceneCompleted;

            var asyncOp = SceneManager.LoadSceneAsync(options.SceneToLoad, options.Mode);

            asyncOp.allowSceneActivation = false;
            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }
            
            if (options.PlayTransition)
            {
                Game.UI.PlayTransitionHalf(() =>
                {
                    if (!options.ActivateOnLoad)
                    {
                        sceneOperations[options.SceneToLoad] = asyncOp;
                    }
                    else
                    {
                        asyncOp.allowSceneActivation = options.ActivateOnLoad;

                        Game.UI.PlayTransitionEnd(() =>
                        {
                            this.onLoadSceneCompleted?.Invoke();
                            this.onLoadSceneCompleted = null;
                        });
                    }

                });
            }
            else
            {
                if (!options.ActivateOnLoad)
                {
                    sceneOperations[options.SceneToLoad] = asyncOp;
                }

                asyncOp.allowSceneActivation = options.ActivateOnLoad;
                
                while (!asyncOp.isDone)
                {
                    yield return null;
                }

                this.onLoadSceneCompleted?.Invoke();
                this.onLoadSceneCompleted = null;
            }
        }

        IEnumerator UnloadSceneCoroutine(string name, Action onUnloadSceneCompleted)
        {
            this.onUnloadSceneCompleted += onUnloadSceneCompleted;

            var asyncOp = SceneManager.UnloadSceneAsync(name);
            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }

            this.onUnloadSceneCompleted?.Invoke();
            this.onUnloadSceneCompleted = null;
        }

        IEnumerator ActivateSceneCoroutine(string name, Action onActivateSceneCompleted)
        {
            this.onActivateSceneCompleted += onActivateSceneCompleted;

            if (sceneOperations.TryGetValue(name, out AsyncOperation asyncOp))
            {
                asyncOp.allowSceneActivation = true;
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
                sceneOperations.Remove(name);
        
            }

            Game.UI.PlayTransitionEnd(() =>
            {
                this.onActivateSceneCompleted?.Invoke();
                this.onActivateSceneCompleted = null;
            });
        }

        #endregion

        public void RegisterSceneObject(GameObject obj)
        {
            currentSceneObjects.Add(obj);
        }

        public void RegisterSceneObjects(List<GameObject> objs)
        {
            currentSceneObjects.AddRange(objs);
        }
                
        public void UnloadSceneObjects()
        {
            foreach (GameObject go in currentSceneObjects)
            {
                Destroy(go);
            }
            currentSceneObjects = new();
        }

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