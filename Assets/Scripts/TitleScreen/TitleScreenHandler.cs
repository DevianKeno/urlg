/*

Component Title: Title Screen Handler
Data written: October 11, 2024
Date revised: October 29, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    

Control:


Data Structures:
    
*/

using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

using RL.Classifiers;
using RL.RD;
using RL.Telemetry;
using RL.UI;

namespace RL.TitleScreen
{
    public class TitleScreenHandler : MonoBehaviour
    {
        public bool EnablePCPCG;
        public bool EnablePCPCGGNB;
        public float ValidateRatio = 0.2f;

        bool exitWindowIsOpen = false;
        
        [Header("Texts")]
        [SerializeField] TextMeshProUGUI datasetFilenameTmp;

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

            dataBtn.onClick.AddListener(OpenSelectDatasetDialog);
            savesBtn.onClick.AddListener(OpenSaves);
            settingsBtn.onClick.AddListener(OpenSettings);
            exitBtn.onClick.AddListener(OpenExitDialog);

            Game.Main.OnLateInit += LateInit;
        }

        void Start()
        {
            pcpcgBtn.interactable = EnablePCPCG;
            pcpcgGnbBtn.interactable = EnablePCPCGGNB;
        }

        void LateInit()
        {
            Game.Main.OnLateInit -= LateInit;
            Game.Audio.PlayMusic("title");
        }

        void PlayPCPCG()
        {
            pcpcgBtn.interactable = false;

            Game.Main.SetAlgorithmAR();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL"
                },
                onLoadSceneCompleted: () =>
                {
                    Game.Audio.StopMusic("title");
                });
        }

        void PlayPCPCG_GNB()
        {
            pcpcgGnbBtn.interactable = false;

            Game.Main.SetAlgorithmGNB();
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LEVEL",
                },
                onLoadSceneCompleted: () =>
                {
                    Game.Audio.StopMusic("title");
                });
        }

        void ResearchDevelopment()
        {
            rdBtn.interactable = false;

            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "R&D"
                },
                onLoadSceneCompleted: () =>
                {
                    Game.Audio.StopMusic("title");
                });
        }

        public void OpenSelectDatasetDialog()
        {
            string datasetsDirectory = Path.Combine(Application.persistentDataPath, "dataset");
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select dataset (.csv)", datasetsDirectory, "csv", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                ParseDatasetContent(CSVHelper.ReadCSV(paths[0]));
                datasetFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
                pcpcgGnbBtn.interactable = true;
            }
        }
        
        GNBData testingSet = null;
        GNBData validationSet = null;
        
        void ParseDatasetContent(List<string[]> content)
        {
            testingSet = new();
            validationSet = new();
            string[] headers = content[0];
            
            for (int i = 1; i < content.Count; i++)
            {
                string[] row = content[i];
                var entry = new ARDataEntry
                {
                    SeedPlayer = int.Parse(row[0]),
                    SeedRoom = int.Parse(row[1]),
                    Values = new Dictionary<StatKey, int>()
                };

                for (int j = 2; j < row.Length; j++)
                    if (Enum.TryParse(headers[j], out StatKey statKey))
                        entry.Values[statKey] = int.Parse(row[j]);

                if (UnityEngine.Random.Range(0, 101) <= ValidateRatio)
                {
                    if (int.Parse(row[^1]) == 1)
                        validationSet.AcceptedEntries.Add(entry);
                    else
                        validationSet.RejectedEntries.Add(entry);
                }
                else
                {
                    if (int.Parse(row[^1]) == 1)
                        testingSet.AcceptedEntries.Add(entry);
                    else
                        testingSet.RejectedEntries.Add(entry);
                }
            }

            GaussianNaiveBayes.Instance.Train(testingSet, validationSet);
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
            if (exitWindowIsOpen) return;
            exitWindowIsOpen = true;

            var exitWindow = Game.UI.Create<ExitGameWindow>("Exit Game Window");
            exitWindow.OnClose += () =>
            {
                exitWindowIsOpen = false;
            };
        }
    }
}