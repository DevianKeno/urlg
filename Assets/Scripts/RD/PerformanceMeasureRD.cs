using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using RL.Telemetry;
using Newtonsoft.Json;
using RL.Classifiers;

namespace RL.RD
{
    public class PerformanceMeasureRD : MonoBehaviour
    {
        ClassifierResultingContainer arResults;
        ClassifierResultingContainer gnbResults;

        [Header("Texts")]
        [SerializeField] TextMeshProUGUI arFilenameTmp;
        [SerializeField] TextMeshProUGUI arInfoTmp;
        [SerializeField] TextMeshProUGUI gnbFilenameTmp;
        [SerializeField] TextMeshProUGUI gnbInfoTmp;
        [SerializeField] TextMeshProUGUI arMetricsTmp;
        [SerializeField] TextMeshProUGUI gnbMetricsTmp;

        [Header("Buttons")]
        [SerializeField] Button arResultsBtn;
        [SerializeField] Button gnbResultsBtn;
        [SerializeField] Button calculateARBtn;
        [SerializeField] Button calculateGNBBtn;

        void Awake()
        {
            arResultsBtn.onClick.AddListener(OpenARResults);
            gnbResultsBtn.onClick.AddListener(OpenGNBResults);
            calculateARBtn.onClick.AddListener(CalculateAR);
            calculateGNBBtn.onClick.AddListener(CalculateGNB);
        }

        void Start()
        {
            arResults = new();
            gnbResults = new();
        }

        #region AR
        void OpenARResults()
        {
            var directory = Path.Combine(Application.persistentDataPath, "results", "ar");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select AR results file (.json)", directory, "dat", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                var content = File.ReadAllText(paths[0]);
                ParseARResults(content);
                arFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }

        void ParseARResults(string content)
        {
            var a = JsonConvert.DeserializeObject<ResultsJsonData>(content);

            foreach (DataEntry entry in a.Entries)
            {
                var status = ConfMatxAnswer(entry.GroundTruth, entry.Classification);
                arResults.IncrementCount(status);
            }
            arResults.TotalEntryCount = a.Entries.Count;
            UpdateARInfoText();
        }

        void CalculateAR()
        {
            float precision = arResults.CalculatePrecision();
            float recall = arResults.CalculateRecall();
            float fScore = arResults.CalculateFScore();

            arMetricsTmp.text = @$"Precision: {precision}
Recall: {recall}
F-Score: {fScore}";
        }

        #endregion
        

        #region GNB

        void OpenGNBResults()
        {
            var directory = Path.Combine(Application.persistentDataPath, "results", "gnb");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select GNB results file (.json)", directory, "dat", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                var content = File.ReadAllText(paths[0]);
                ParseGNBResults(content);
                gnbFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }
        
        void ParseGNBResults(string content)
        {
            var a = JsonConvert.DeserializeObject<ResultsJsonData>(content);

            foreach (DataEntry entry in a.Entries)
            {
                var status = ConfMatxAnswer(entry.GroundTruth, entry.Classification);
                gnbResults.IncrementCount(status);
            }
            gnbResults.TotalEntryCount = a.Entries.Count;
            UpdateGNBInfoText();
        }

        void CalculateGNB()
        {
            float precision = gnbResults.CalculatePrecision();
            float recall = gnbResults.CalculateRecall();
            float fScore = gnbResults.CalculateFScore();

            gnbMetricsTmp.text = @$"Precision: {precision}
Recall: {recall}
F-Score: {fScore}";
        }

        #endregion


        ConfusionMatrixStatus ConfMatxAnswer(int groundTruth, int classification)
        {
            bool liked = groundTruth == 1;
            bool accepted = classification == 1;
            
            if (liked && accepted)
                return ConfusionMatrixStatus.TruePositive;

            else if (!liked && !accepted)
                return ConfusionMatrixStatus.TrueNegative;

            else if (liked && !accepted)
                return ConfusionMatrixStatus.FalseNegative;

            else if (!liked && accepted)
                return ConfusionMatrixStatus.FalsePositive;

            else
                return ConfusionMatrixStatus.Invalid;
        }

        void UpdateARInfoText()
        {
            arInfoTmp.text = @$"# of entries: {arResults.TotalEntryCount}
TP: {arResults.TPCount}
TN: {arResults.TNCount}
FP: {arResults.FPCount}
FN: {arResults.FNCount}
            ";
        }

        void UpdateGNBInfoText()
        {
            gnbInfoTmp.text = @$"# of entries: {gnbResults.TotalEntryCount}
TP: {gnbResults.TPCount}
TN: {gnbResults.TNCount}
FP: {gnbResults.FPCount}
FN: {gnbResults.FNCount}
            ";
        }

        public void Calculate()
        {
            gnbMetricsTmp.text = @$"Precision: {5}
Recall:
F-score:";
        }
    }
}