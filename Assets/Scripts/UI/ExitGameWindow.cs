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
            Destroy(gameObject);
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