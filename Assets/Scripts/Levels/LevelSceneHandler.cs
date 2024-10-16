using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL.Levels
{
    public class LevelSceneHandler : MonoBehaviour
    {
        public Level Level;
        public List<GameObject> SceneObjects = new();

        [SerializeField] TextMeshProUGUI algorithmTmp;
        [SerializeField] TextMeshProUGUI levelNumberTmp;

        bool isLevelGenerated = false;
        bool telemetryIsVisible = false;
        
        void Awake()
        {
            Level = GetComponent<Level>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                
            }
        }

        void Start()
        {
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LOADING",
                    Mode = LoadSceneMode.Additive ,
                    PlayTransition = false, },
                onLoadSceneCompleted: () =>
                {
                    Initialize();
                });
        }

        public void Initialize()
        {
            if (isLevelGenerated) return;

            if (Game.Main.AlgorithmUsed == PCGAlgorithm.AcceptReject)
            {
                algorithmTmp.text = "PCPCG";
            }
            else if (Game.Main.AlgorithmUsed == PCGAlgorithm.GaussianNaiveBayes)
            {
                algorithmTmp.text = "PCPCG-GNB";
            }

            levelNumberTmp.text = $"Level <b>{Game.Main.currentLevel}";

            Level.Initialize();
        }
    }
}