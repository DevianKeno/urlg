/*

Component Title: Exit Game Window
Data written: October 5, 2024
Date revised: October 26, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Implementation of the exit game window.

Data Structures:
    N/A
*/

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class ExitGameWindow : Window
    {
        [SerializeField] Button backBtn;
        [SerializeField] Button exitBtn;

        void Start()
        {
            backBtn.onClick.AddListener(Back);
            exitBtn.onClick.AddListener(ExitGame);
        }

        public void Back()
        {
            Hide(destroy: true);
        }

        public void ExitGame()
        {
            #if UNITY_EDITOR
                // Stop playing the scene in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                // Quit the application when it's built
                Application.Quit();
            #endif
        }
    }
}