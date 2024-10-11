using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL.Levels
{
    public class LevelSceneHandler : MonoBehaviour
    {
        bool isLevelGenerated = false;
        public Level Level;
        
        void Awake()
        {
            Level = GetComponent<Level>();
        }

        void Start()
        {
            if (isLevelGenerated) return;

            Level.Initialize();
        }
    }
}