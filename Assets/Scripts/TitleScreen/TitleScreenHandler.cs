using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RL.Levels;

namespace RL.TitleScreen
{
    public class TitleScreenHandler : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button pcpcgBtn;
        [SerializeField] Button pcpcgGnbBtn;
        [SerializeField] Button rdBtn;
        [SerializeField] Button dataBtn;
        [SerializeField] Button savesBtn;
        [SerializeField] Button settingsBtn;
        [SerializeField] Button exitBtn;

        void Awake()
        {
            pcpcgBtn.onClick.AddListener(PlayPCPCG);
            pcpcgGnbBtn.onClick.AddListener(PlayPCPCG_GNB);
            rdBtn.onClick.AddListener(ResearchDevelopment);

            dataBtn.onClick.AddListener(OpenData);
            savesBtn.onClick.AddListener(OpenSaves);
            settingsBtn.onClick.AddListener(OpenSettings);
            exitBtn.onClick.AddListener(OpenExitDialog);
        }

        void PlayPCPCG()
        {
            Game.Main.SetAlgorithmAR();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                    PlayTransition = true, },
                onLoadSceneCompleted: () =>
                {
                    Game.Main.LoadScene(
                        new(){
                            SceneToLoad = "LOADING",
                            Mode = LoadSceneMode.Additive },
                            onLoadSceneCompleted: () =>
                            {
                                Game.Main.ActivateScene(
                                    "LEVEL",
                                    onActivateSceneCompleted: () =>
                                    {
                                        LevelSceneHandler.Instance.Initialize();
                                    });
                            });
                });
            
        }

        void PlayPCPCG_GNB()
        {
            Game.Main.SetAlgorithmGNB();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                });
            // Game.Main.LoadScene(
            //     new(){
            //         SceneToLoad = "LOADING",
            //         Mode = LoadSceneMode.Additive
            //     });
        }

        void ResearchDevelopment()
        {
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "R&D",
                });
        }

        void OpenData()
        {
            Game.Files.OpenDatasFolder();
        }

        void OpenSaves()
        {
            Game.Files.OpenSavesFolder();
        }
        
        void OpenSettings()
        {
            Game.Main.OpenSettingsMenu();
        }

        void OpenExitDialog()
        {
            
        }
    }
}