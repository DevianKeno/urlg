using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;

using RL.Telemetry;
using RL.Classifiers;
using RL.UI;
using RL.RD.UI;
using RL.Graphs;
using RL.CellularAutomata;

namespace RL.RD
{
    public class GaussianNaiveBayesRD : MonoBehaviour
    {
        public const int MaxBulkGenerationTimes = 1000;
        public const string AcceptedMessage = @"The generated room matches the player's preferences,
therefore is <b>accepted</b>.";
        public const string RejectedMessage = @"The generated room falls out of the player's preferences,
therefore is <b>rejected</b>.";

        public MockRoom Room = null;
        public bool NormalizeValues => normalizeToggle != null ? normalizeToggle.isOn : false;

        [SerializeField] bool _hasDataset = false;
        GNBData data = null;
        
        /// <summary>
        /// The current state of Player Stats.
        /// </summary>
        PlayerStatCollection playerStats;
        /// <summary>
        /// The current state of Room Stats.
        /// </summary>
        RoomStatCollection roomStats;
        GNBResult _previousResult;

        [SerializeField] RDTelemetryUI playerTelemetryUI;
        [SerializeField] RDTelemetryUI roomTelemetryUI;
        [SerializeField] ConfusionMatrixHandler confusionMatrixHandler;

        [Header("Bulk Gen Settings")]
        public Status BulkGenTarget = Status.Accepted;
        public bool VisualizeBulkGen = true;
        public float BulkGenDelay = 0f;
        System.Diagnostics.Stopwatch bulkGenTimer;
        
        [Header("Graphs")]
        [SerializeField] ARGraph fireGraph;
        [SerializeField] ARGraph beamGraph;
        [SerializeField] ARGraph waveGraph;
        [SerializeField] ARGraph skillGraph;
        
        [Header("Buttons")]
        [SerializeField] Button datasetBtn;
        [SerializeField] Button setValuesBtn;
        [SerializeField] Button bulkGenBtn;
        [SerializeField] Button generateRoomBtn;
        [SerializeField] Toggle normalizeToggle;
        [SerializeField] Button likedBtn;
        [SerializeField] Button dislikedBtn;

        [Header("Texts")]
        [SerializeField] TextMeshProUGUI iterationsTmp;
        [SerializeField] TextMeshProUGUI timeTmp;
        [SerializeField] TextMeshProUGUI acceptedTmp;
        [SerializeField] TextMeshProUGUI rejectedTmp;
        [SerializeField] TextMeshProUGUI messageTmp;
        [SerializeField] TextMeshProUGUI datasetFilenameTmp;
        [SerializeField] TextMeshProUGUI datasetMessageTmp;
        [SerializeField] TextMeshProUGUI modelDataMeanTmp;
        [SerializeField] TextMeshProUGUI modelDataVarianceTmp;

        void Awake()
        {
            datasetBtn.onClick.AddListener(SelectDataset);

            likedBtn.onClick.AddListener(LikeFeatureSet);
            dislikedBtn.onClick.AddListener(DislikeFeatureSet);
        }
        void Start()
        {
            ClearAllPoints();
            ResetARStatus();
            datasetMessageTmp.gameObject.SetActive(false);
            bulkGenTimer = new();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ClassifyGNB();
            }
        }


        #region Public methods

        public void ClassifyGNB()
        {
            if (!_hasDataset)
            {
                datasetMessageTmp.gameObject.SetActive(true);
                datasetMessageTmp.text = $"<color=#{hexRed}>Select a dataset";
                Debug.LogError("Provide a dataset");
                return;
            }

            var result = GaussianNaiveBayes.Instance.ClassifyRoom(playerStats, roomStats);
            if (result.Status == Status.Accepted)
            {
                SetDisplayAccepted();
            }
            else if (result.Status == Status.Rejected)
            {
                SetDisplayRejected();
            }

            _previousResult = result;
            AppendPosteriorText();
        }

        void AppendPosteriorText()
        {
            acceptedTmp.text += $": {_previousResult.PosteriorAccepted:0.0#####}";
            rejectedTmp.text += $": {_previousResult.PosteriorRejected:0.0#####}";
        }

        bool _continueBulkGeneration;

        public void BulkGenerateUntilStatus()
        {
            _continueBulkGeneration = true;
            bulkGenBtn.onClick.RemoveAllListeners();
            bulkGenBtn.onClick.AddListener(StopBulkGenerate);
            var tmp = bulkGenBtn.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = "STOP";
            tmp.color = Color.red;
            iterationsTmp.gameObject.SetActive(true);
            StartCoroutine(BulkGenerateUntilStatusCoroutine());
        }

        public void StopBulkGenerate()
        {
            _continueBulkGeneration = false;
            SetButtonBulk();
            StopAllCoroutines();
        }

        void SetButtonBulk()
        {
            bulkGenBtn.onClick.RemoveAllListeners();
            bulkGenBtn.onClick.AddListener(BulkGenerateUntilStatus);
            var tmp = bulkGenBtn.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = "BULK";
            tmp.color = Color.white;
        }

        IEnumerator BulkGenerateUntilStatusCoroutine()
        {
            string message = $"Iterations: ";

            bulkGenTimer.Reset();
            timeTmp.text = $"Time: {bulkGenTimer.ElapsedMilliseconds} ms";
            SetDisplayRejected();
            bulkGenTimer.Start();

            for (int i = 0; i < MaxBulkGenerationTimes; i++)
            {
                roomTelemetryUI.RandomizeRoomFeatures();
                SetRoomValues();
                _previousResult = GaussianNaiveBayes.Instance.ClassifyRoom(playerStats, roomStats);                
                iterationsTmp.text = message + i;
                if (!_continueBulkGeneration ||_previousResult.Status == BulkGenTarget) break;
                if (VisualizeBulkGen) yield return new WaitForSeconds(BulkGenDelay);
            }

            bulkGenTimer.Stop();
            timeTmp.text = $"Time ms: {bulkGenTimer.ElapsedMilliseconds} ms";
            if (_previousResult.Status == Status.Accepted)
            {
                SetDisplayAccepted();
            } else if (_previousResult.Status == Status.Rejected)
            {
                SetDisplayRejected();
            }
            AppendPosteriorText();
            SetButtonBulk();
            yield return null;
        }

        public void SetPlayerValues()
        {
            playerStats = playerTelemetryUI.ConstructPlayerTelemetryStats();

            var firePref = Evaluate.Player.WeaponPreference(StatKey.HitCountFire, StatKey.UseCountFire, playerStats);
            var beamPref = Evaluate.Player.WeaponPreference(StatKey.HitCountBeam, StatKey.UseCountBeam, playerStats);
            var wavePref = Evaluate.Player.WeaponPreference(StatKey.HitCountWave, StatKey.UseCountWave, playerStats);

            if (NormalizeValues)
            {
                Math.NormalizeMaxed(ref firePref, ref beamPref, ref wavePref);
            }

            fireGraph.SetBoundsY(0f, (float) firePref);
            beamGraph.SetBoundsY(0f, (float) beamPref);
            waveGraph.SetBoundsY(0f, (float) wavePref);
        }

        public void SetRoomValues()
        {
            roomStats = roomTelemetryUI.ConstructRoomTelemetryStats();

            var firePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountFire, StatKey.ObstacleCountFire, roomStats);
            var beamPref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountBeam, StatKey.ObstacleCountBeam, roomStats);
            var wavePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountWave, StatKey.ObstacleCountWave, roomStats);
            
            if (NormalizeValues)
            {
                Math.NormalizeMaxed(ref firePref, ref beamPref, ref wavePref);
            }

            ClearAllPoints();
            fireGraph.PlotPoint((float) firePref);
            beamGraph.PlotPoint((float) beamPref);
            waveGraph.PlotPoint((float) wavePref);
        }

        public void PlotPoints()
        {
            if (Room == null) return;

            var stats = Room.Stats;
            var firePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountFire, StatKey.ObstacleCountFire, stats);
            var beamPref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountBeam, StatKey.ObstacleCountBeam, stats);
            var wavePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountWave, StatKey.ObstacleCountWave, stats);

            ClearAllPoints();
            fireGraph.PlotPoint((float) firePref);
            beamGraph.PlotPoint((float) beamPref);
            waveGraph.PlotPoint((float) wavePref);
        }

        #endregion

        public void ClearAllPoints()
        {
            fireGraph.RemovePoints();
            beamGraph.RemovePoints();
            waveGraph.RemovePoints();
        }

        string hexGray = "4B4B4B";
        string hexGreen = "40D945";
        string hexRed = "F62B2B";
        public void SetDisplayAccepted()
        {
            acceptedTmp.text = $"<color=#{hexGreen}>Accepted";
            rejectedTmp.text = $"<color=#{hexGray}>Rejected";
            messageTmp.text = AcceptedMessage;
        }

        public void SetDisplayRejected()
        {
            acceptedTmp.text = $"<color=#{hexGray}>Accepted";
            rejectedTmp.text = $"<color=#{hexRed}>Rejected";
            messageTmp.text = RejectedMessage;
        }

        public void ResetARStatus()
        {
            acceptedTmp.text = $"<color=#{hexGray}>Accepted";
            rejectedTmp.text = $"<color=#{hexGray}>Rejected";
            messageTmp.text = "";
        }
        
        public void LikeFeatureSet()
        {
            if (_previousResult.Status == Status.Accepted)
            {
                confusionMatrixHandler.SetValue(ConfusionMatrixValue.TruePositive);
            }
            else if (_previousResult.Status == Status.Rejected)
            {
                confusionMatrixHandler.SetValue(ConfusionMatrixValue.FalseNegative);
            }
        }

        public void DislikeFeatureSet()
        {
            if (_previousResult.Status == Status.Accepted)
            {
                confusionMatrixHandler.SetValue(ConfusionMatrixValue.FalsePositive);
            }
            else if (_previousResult.Status == Status.Rejected)
            {
                confusionMatrixHandler.SetValue(ConfusionMatrixValue.TrueNegative);
            }
        }

        public void SelectDataset()
        {
            string datasetsDirectory = Path.Combine(Application.persistentDataPath, "datasets");
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select dataset (.csv)", datasetsDirectory, "csv", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                datasetMessageTmp.gameObject.SetActive(false);
                List<string[]> content = CSVHelper.ReadCSV(paths[0]);
                ParseDatasetContent(content);
                datasetFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }

        void ParseDatasetContent(List<string[]> content)
        {
            data = new();
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

                if (int.Parse(row[^1]) == 1)
                    data.AcceptedEntries.Add(entry);
                else
                    data.RejectedEntries.Add(entry);
            }

            _hasDataset = true;
            GaussianNaiveBayes.Instance.Train(data);
        }
    }
}