using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RL.RD
{
    public class RDSceneHandler : MonoBehaviour
    {
        public List<GameObject> SceneObjects;
        public Button exitBtn;
        
        void Start()
        {
            exitBtn.onClick.AddListener(ToTitle);
            Game.Main.RegisterSceneObjects(SceneObjects);
        }

        void ToTitle()
        {
            Game.Main.SetAlgorithmAR();
            Game.Main.UnloadSceneObjects();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "TITLE",
                });
        }
    }
}