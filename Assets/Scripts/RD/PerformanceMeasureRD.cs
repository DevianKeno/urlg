using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using RL.Telemetry;
using Newtonsoft.Json;
using RL.Classifiers;
using System;
using System.Linq;
using System.Text;

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
        [SerializeField] TextMeshProUGUI dataTmp;
        [SerializeField] TextMeshProUGUI meanTmp;


        [Header("Buttons")]
        [SerializeField] Button arResultsBtn;
        [SerializeField] Button arFolderBtn;
        [SerializeField] Button gnbResultsBtn;
        [SerializeField] Button gnbFolderBtn;
        [SerializeField] Button calculateARBtn;
        [SerializeField] Button calculateGNBBtn;
        [SerializeField] Button saveARBtn;
        [SerializeField] Button saveGNBBtn;
        [SerializeField] Button calculateMeanBtn;
        [SerializeField] Button exportBtn;

        [Header("Other")]
        [SerializeField] Transform dataContainer;
        [SerializeField] GameObject dataEntryPrefab;

        private int playerIdCounter = 1;

        private List<float> arPrecisions = new List<float>();
        private List<float> arRecalls = new List<float>();
        private List<float> arFScores = new List<float>();

        private List<float> gnbPrecisions = new List<float>();
        private List<float> gnbRecalls = new List<float>();
        private List<float> gnbFScores = new List<float>();

        void Awake()
        {
            arResultsBtn.onClick.AddListener(OpenARResults);
            arFolderBtn.onClick.AddListener(OpenARFolder);

            gnbResultsBtn.onClick.AddListener(OpenGNBResults);
            gnbFolderBtn.onClick.AddListener(OpenGNBFolder);

            calculateARBtn.onClick.AddListener(CalculateAR);
            calculateGNBBtn.onClick.AddListener(CalculateGNB);

            saveARBtn.onClick.AddListener(SaveARMetrics);
            saveGNBBtn.onClick.AddListener(SaveGNBMetrics);

            calculateMeanBtn.onClick.AddListener(CalculateMean);
            exportBtn.onClick.AddListener(ExportToCSV);
        }

        void Start()
        {
            arResults = new();
            gnbResults = new();
        }

        #region AR
        void OpenARResults()
        {
            arResults.TotalEntryCount = 0;
            arResults = new();
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

        void OpenARFolder()
        {
            arResults.TotalEntryCount = 0;
            arResults = new();
            var directory = Path.Combine(Application.persistentDataPath, "results", "ar");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            var filePaths = Directory.EnumerateFiles(directory)
                                .Where(file => file.EndsWith(".json") || file.EndsWith(".dat"));
            foreach (var filePath in filePaths)
            {
                try
                {
                    var content = File.ReadAllText(filePath);
                    ParseARResults(content);
                }
                catch
                {
                    continue;
                }
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
            arResults.TotalEntryCount += a.Entries.Count;
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
        void SaveARMetrics()
        {
            string playerId = $"Player {playerIdCounter}";

            float precision = arResults.CalculatePrecision();
            float recall = arResults.CalculateRecall();
            float fScore = arResults.CalculateFScore();

            arPrecisions.Add(precision);
            arRecalls.Add(recall);
            arFScores.Add(fScore);

            string displayText = $"{playerId}      Accept/Reject     {precision:F6}     {recall:F6}     {fScore:F6}";

            GameObject newEntry = Instantiate(dataEntryPrefab, dataContainer);
            TextMeshProUGUI dataTMP = newEntry.GetComponentInChildren<TextMeshProUGUI>();

            dataTMP.text = displayText;
            playerIdCounter++;
        }

        #endregion
        

        #region GNB

        void OpenGNBResults()
        {
            gnbResults.TotalEntryCount = 0;
            gnbResults = new();

            var directory = Path.Combine(Application.persistentDataPath, "results", "gnb");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select GNB results file (.json)", directory, "dat", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                var content = File.ReadAllText(paths[0]);
                ParseARResults(content);
                gnbFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }
        
        void OpenGNBFolder()
        {
            gnbResults.TotalEntryCount = 0;
            gnbResults = new();

            var directory = Path.Combine(Application.persistentDataPath, "results", "gnb");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            var filePaths = Directory.EnumerateFiles(directory)
                                .Where(file => file.EndsWith(".json") || file.EndsWith(".dat"));
            foreach (var filePath in filePaths)
            {
                try
                {
                    var content = File.ReadAllText(filePath);
                    ParseGNBResults(content);
                }
                catch
                {
                    continue;
                }
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

        void SaveGNBMetrics()
        {
            string playerId = $"Player {playerIdCounter}";

            float precision = gnbResults.CalculatePrecision();
            float recall = gnbResults.CalculateRecall();
            float fScore = gnbResults.CalculateFScore();

            gnbPrecisions.Add(precision);
            gnbRecalls.Add(recall);
            gnbFScores.Add(fScore);

            string displayText = @$"{playerId}            GNB             {precision:F6}     {recall:F6}     {fScore:F6}";

            GameObject newEntry = Instantiate(dataEntryPrefab, dataContainer);
            TextMeshProUGUI dataTMP = newEntry.GetComponentInChildren<TextMeshProUGUI>();

            dataTMP.text = displayText;
            playerIdCounter++;
        }

        void CalculateMean()
        {
            float arMeanPrecision = arPrecisions.Count > 0 ? arPrecisions.Average() : 0;
            float arMeanRecall = arRecalls.Count > 0 ? arRecalls.Average() : 0;
            float arMeanFScore = arFScores.Count > 0 ? arFScores.Average() : 0;

            float gnbMeanPrecision = gnbPrecisions.Count > 0 ? gnbPrecisions.Average() : 0;
            float gnbMeanRecall = gnbRecalls.Count > 0 ? gnbRecalls.Average() : 0;
            float gnbMeanFScore = gnbFScores.Count > 0 ? gnbFScores.Average() : 0;

            meanTmp.text = @$"   {arMeanPrecision:F6}     {arMeanRecall:F6}     {arMeanFScore:F6}
   {gnbMeanPrecision:F6}     {gnbMeanRecall:F6}     {gnbMeanFScore:F6}";
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

        public void ExportToCSV()
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Player ID,Algorithm,Precision,Recall,F-Score");

            // Add saved Accept/Reject metrics
            for (int i = 0; i < arPrecisions.Count; i++)
            {
                csvContent.AppendLine($"{i + 1},Accept/Reject,{arPrecisions[i]:F6},{arRecalls[i]:F6},{arFScores[i]:F6}");
            }

            // Add saved GNB metrics
            for (int i = 0; i < gnbPrecisions.Count; i++)
            {
                csvContent.AppendLine($"{i + 1},GNB,{gnbPrecisions[i]:F6},{gnbRecalls[i]:F6},{gnbFScores[i]:F6}");
            }

            // Define file path and save the file
            string filePath = Path.Combine(Application.persistentDataPath, "PerformanceMetrics.csv");
            File.WriteAllText(filePath, csvContent.ToString());

            Debug.Log($"Exported to {filePath}");
        }

    }
}