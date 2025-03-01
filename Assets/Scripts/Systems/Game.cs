/*
Program Title: Game (URLG)
Data written: June 18, 2024
Date revised: December 17, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This is the main singleton class that manages the entire Game application.
    Houses all the game managers to control and manage the different parts of the game.
    Contains helper functions for scene management.

Control:
    Awake()
        -> <manager>.Initialize()
        -> StartTrain()

    The program is initialized at the start of the application.
    If enabled (true by default), this is also the time where the GNB model is trained using the provided dataset.

Data Structures:

*/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using RL.Systems;
using RL.CellularAutomata;
using RL.Levels;
using RL.Player;
using RL.Weapons;
using RL.Classifiers;
using RL.RD;
using RL.Telemetry;

namespace RL
{
    public enum PCGAlgorithm { AcceptReject, GaussianNaiveBayes }
    
    public class Game : MonoBehaviour
    {
        #region Constants
        public const int MaxEnemiesPerRoom = 12;
        public const float FireTickSeconds = 1f;
        public const int BurnDamage = 10;
        
        #endregion
        
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
        public bool UseTestLevel = false;//BROKEN
        public Room CurrentRoom;
        public bool TrainGnbModelOnStart = false;
        public PlayerController Player = null;
        public Weapon PlayerEquippedWeapon1 = null;
        public Weapon PlayerEquippedWeapon2 = null;
        public Weapon PlayerUnequippedWeapon = null;

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
            // telemetry.Initialize();
            tilesManager.Initialize();
            entityManager.Initialize();
            particlesManager.Initialize();

            OnLateInit?.Invoke();

            if (TrainGnbModelOnStart)
            {
                StartTrain();
            }
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

        
        public void StartTrain()
        {
            Debug.Log("Start train of Model instance");

            var datasetFilepath = Path.Combine(Application.dataPath, "Resources", "Dataset", "1.csv");
            var content = CSVHelper.ReadCSV(datasetFilepath);
            ParseDatasetContent(content);
        }

        void ParseDatasetContent(List<string[]> content)
        {
            var testingSet = new GNBData();
            var validationSet = new GNBData();
            string[] headers = content[0];
            
            for (int i = 1; i < content.Count; i++)
            {
                string[] row = content[i];
                var entry = new ARDataEntry
                {
                    SeedPlayer = int.Parse(row[0]),
                    SeedRoom = int.Parse(row[1]),
                    Values = new Dictionary<StatKey, int>()
                };

                for (int j = 2; j < row.Length; j++)
                    if (Enum.TryParse(headers[j], out StatKey statKey))
                        entry.Values[statKey] = int.Parse(row[j]);

                if (UnityEngine.Random.Range(0, 101) <= (20))
                {
                    if (int.Parse(row[^1]) == 1)
                        validationSet.AcceptedEntries.Add(entry);
                    else
                        validationSet.RejectedEntries.Add(entry);
                }
                else
                {
                    if (int.Parse(row[^1]) == 1)
                        testingSet.AcceptedEntries.Add(entry);
                    else
                        testingSet.RejectedEntries.Add(entry);
                }
            }

            GaussianNaiveBayes.Instance.Train(testingSet, validationSet);
            Debug.Log("Model instance trained");
        }
    }
}