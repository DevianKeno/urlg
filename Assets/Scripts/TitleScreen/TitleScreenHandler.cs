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

        void Start()
        {
            pcpcgBtn.interactable = true;
            pcpcgGnbBtn.interactable = true;
        }

        void PlayPCPCG()
        {
            pcpcgBtn.interactable = false;
            
            Game.Main.SetAlgorithmAR();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL"});
        }

        void PlayPCPCG_GNB()
        {
            pcpcgGnbBtn.interactable = false;

            Game.Main.SetAlgorithmGNB();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                });
        }

        void ResearchDevelopment()
        {
            rdBtn.interactable = false;

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