using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL.Levels
{
    public class LevelSceneHandler : MonoBehaviour
    {
        public Level Level;
        public List<GameObject> SceneObjects = new();

        bool isLevelGenerated = false;
        
        void Awake()
        {
            Level = GetComponent<Level>();
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

            Level.Initialize();
        }
    }
}