using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RL.RD
{
    public class RDSceneHandler : MonoBehaviour
    {
        public Button exitBtn;
        
        void Start()
        {
            exitBtn.onClick.AddListener(ToTitle);
        }

        void ToTitle()
        {
            Game.Main.SetAlgorithmAR();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "TITLE",
                });
        }
    }
}