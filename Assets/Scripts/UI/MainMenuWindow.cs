using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RL.UI
{
    public class MainMenuWindow : Window
    {
        [SerializeField] Button backBtn;
        [SerializeField] Button exitBtn;

        void Start()
        {
            backBtn.onClick.AddListener(Back);
            exitBtn.onClick.AddListener(MainMenu);
        }

        public void Back()
        {
            Hide(destroy: true);
        }

        public void MainMenu()
        {
            exitBtn.interactable = false;
            Game.Telemetry.SaveEntriesToJson();

            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "TITLE",
                    Mode = LoadSceneMode.Additive,
                    PlayTransition = true, },
                onLoadSceneCompleted: () =>
                {
                    Game.Main.UnloadScene(
                        "LEVEL",
                        onUnloadSceneCompleted: () =>
                        {
                            
                        });
                });
                
            Hide(destroy: true);
        }
    }
}