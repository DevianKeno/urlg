using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RL.TitleScreen
{
    public class TitleScreenHandler : MonoBehaviour
    {
        [SerializeField] Button pcpcgBtn;
        [SerializeField] Button pcpcgGnbBtn;
        [SerializeField] Button rdBtn;
        [SerializeField] Button dataBtn;
        [SerializeField] Button savesBtn;
        [SerializeField] Button settingsBtn;

        void Start()
        {
            pcpcgBtn.onClick.AddListener(PlayPCPCG);
            pcpcgGnbBtn.onClick.AddListener(PlayPCPCG_GNB);
            rdBtn.onClick.AddListener(ResearchDevelopment);
            dataBtn.onClick.AddListener(OpenData);
            savesBtn.onClick.AddListener(OpenSaves);
            settingsBtn.onClick.AddListener(OpenSettings);
        }

        void PlayPCPCG()
        {
            Game.Main.SetAlgorithmAR();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                });
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LOADING",
                    Mode = LoadSceneMode.Additive
                });
        }

        void PlayPCPCG_GNB()
        {
            Game.Main.SetAlgorithmGNB();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                });
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LOADING",
                    Mode = LoadSceneMode.Additive
                });
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
    }
}