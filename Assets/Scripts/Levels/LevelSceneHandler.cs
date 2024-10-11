using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL.Levels
{
    public class LevelSceneHandler : MonoBehaviour
    {
        public static LevelSceneHandler Instance { get; private set;}
        public Level Level;
        public List<GameObject> SceneObjects = new();

        bool isLevelGenerated = false;
        
        void Awake()
        {
            DontDestroyOnLoad(this);
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
            
            Level = GetComponent<Level>();
        }

        public void Initialize()
        {
            if (isLevelGenerated) return;

            Level.Initialize();
        }
    }
}