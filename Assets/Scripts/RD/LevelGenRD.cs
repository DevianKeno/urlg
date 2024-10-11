using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

using RL.UI;
using RL.Telemetry;
using RL.CellularAutomata;
using RL.Levels;
using RL.Classifiers;
using RL.Graphs;

namespace RL.RD
{
    public class LevelGenRD : MonoBehaviour
    {
        const int DefaultPixelsPerUnit = 120;

        public PCGAlgorithm SelectedAlgorithm = PCGAlgorithm.AcceptReject;
        public RecolorType RecolorType = RecolorType.BOTH;

        [SerializeField] bool _hasDataset = false;
        public bool NormalizeValues => normalizeToggle != null ? normalizeToggle.isOn : false;
        [Range(0, 100)] public float RejectedRoomsThreshold = 25f;
        public float AcceptanceThreshold = 0f;
        GNBData data = null;
        PlayerStatCollection playerStats;
        
        [SerializeField] RDTelemetryUI levelSettings;
        [SerializeField] RDTelemetryUI playerTelemetry;
        
        [Header("Objects")]
        [SerializeField] GameObject playerGraphs;
        [SerializeField] GameObject selector;

        [Header("Graphs")]
        [SerializeField] ARGraph fireGraph;
        [SerializeField] ARGraph beamGraph;
        [SerializeField] ARGraph waveGraph;
        [SerializeField] ARGraph skillGraph;

        [Header("Buttons")]
        [SerializeField] Button pcpcgBtn;
        [SerializeField] Button pcpcgGnbBtn;
        [SerializeField] Button playerGraphsBtn;
        [SerializeField] Button setValuesBtn;
        [SerializeField] Button generateBtn;
        [SerializeField] Button datasetBtn;
        [SerializeField] Toggle normalizeToggle;

        [Header("Texts")]
        [SerializeField] TextMeshProUGUI selectedAlgoTmp;
        [SerializeField] TextMeshProUGUI featureDataTmp;
        [SerializeField] TextMeshProUGUI datasetFilenameTmp;

        void Awake()
        {
            pcpcgBtn.onClick.AddListener(() =>
            {
                SelectAcceptReject();
                HidePlayerGraphs();
                normalizeToggle.gameObject.SetActive(true);
            });
            pcpcgGnbBtn.onClick.AddListener(() =>
            {
                SelectGNB();
                HidePlayerGraphs();
                normalizeToggle.gameObject.SetActive(false);
            });

            setValuesBtn.onClick.AddListener(SetPlayerValues);
            
            generateBtn.onClick.AddListener(GenerateRooms);
            datasetBtn.onClick.AddListener(OpenDatasetDialog);
        }

        void Start()
        {
            datasetBtn.gameObject.SetActive(false);
            normalizeToggle.gameObject.SetActive(false);
            ResetFeatureDataText();
        }


        #region Public methods

        public void SetPlayerValues()
        {
            playerStats = playerTelemetry.ConstructPlayerTelemetryStats();

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

        public void ShowPlayerGraphs()
        {
            playerGraphs.transform.localPosition = Vector3.zero;
        }
        
        public void HidePlayerGraphs()
        {
            playerGraphs.transform.localPosition = new(1920f, 0f, 0f);
        }

        #endregion


        void GenerateRooms()
        {
            int roomCount = levelSettings.GetEntry(StatKey.RoomCount).Value;
            if (roomCount % 2 != 0) roomCount++;
            var generatedRooms = Game.CA.GenerateRooms(roomCount, featurize: false);

            foreach (MockRoom room in generatedRooms)
            {
                room.OnClick += OnClickRoom;

                if (room.IsStartRoom || room.IsEndRoom) continue;

                Status targetStatus = Status.None;
                int rand = UnityEngine.Random.Range(0, 100);
                if (rand > RejectedRoomsThreshold)
                    targetStatus = Status.Accepted;
                else
                    targetStatus = Status.Rejected;

                Featurize(SelectedAlgorithm, room, targetStatus);
                room.Recolor(RecolorType);
            }
        }

        void Featurize(PCGAlgorithm algorithm, MockRoom room, Status targetStatus)
        {
            if (room == null) return;

            if (algorithm == PCGAlgorithm.AcceptReject)
            {
                FeaturizeAR(room, targetStatus);
            }
            else if (algorithm == PCGAlgorithm.GaussianNaiveBayes)
            {
                FeaturizeGNB(room, targetStatus);
            }
        }

        const int MaxAttempts = 255;
        const int MaxBulkGenerationTimes = 1000;
        void FeaturizeAR(MockRoom room, Status targetStatus)
        {   
            var playerStats = playerTelemetry.ConstructPlayerTelemetryStats();
            RoomStatCollection roomStats;
            ARResult previousResult;

            int attempts = 0;
            do
            {
                roomStats = RDTelemetryUI.ConstructRoomRandom(
                    levelSettings.GetEntry(StatKey.MaxEnemyCount).Value,
                    levelSettings.GetEntry(StatKey.MaxObstacleCount).Value);

                previousResult = ARClassifier.Classify(playerStats, roomStats, 0.2f, normalized: true);

                if (previousResult.Status == targetStatus) break;
                attempts++;
            } while (attempts < MaxBulkGenerationTimes);

            room.Featurize(roomStats);
            room.ClassificationStatus = targetStatus;
        }

        void FeaturizeGNB(MockRoom room, Status targetStatus)
        {
            var playerStats = playerTelemetry.ConstructPlayerTelemetryStats();
            RoomStatCollection roomStats;
            GNBResult previousResult;

            int attempts = 0;
            do
            {
                roomStats = RDTelemetryUI.ConstructRoomRandom(
                    levelSettings.GetEntry(StatKey.MaxEnemyCount).Value,
                    levelSettings.GetEntry(StatKey.MaxObstacleCount).Value);

                previousResult = GaussianNaiveBayes.Instance.ClassifyRoom(playerStats, roomStats);  
                if (previousResult.Status == targetStatus) break;
                attempts++;
            } while (attempts < MaxBulkGenerationTimes);

            room.Featurize(roomStats);
            room.ClassificationStatus = targetStatus;
        }

        void SelectAcceptReject()
        {
            selectedAlgoTmp.text = "Selected algorithm: PCPCG";
        }

        void SelectGNB()
        {
            selectedAlgoTmp.text = "Selected algorithm: PCPCG-GNB";
            if (!_hasDataset)
            {
                OpenDatasetDialog();
            }
        }
        
        void OnClickRoom(MockRoom room)
        {
            SetFeatureDataText(room);
            SelectorToRoom(room);
        }

        void SelectorToRoom(MockRoom room)
        {
            if (selector == null)
            {
                selector = Instantiate(Resources.Load<GameObject>("Prefabs/RD/Selector"), transform);
            }
            LeanTween.cancel(selector);
            LeanTween.scale(selector, Vector3.zero, 0f);
            LeanTween.scale(selector, new(1.05f, 1.05f, 1f), 0.05f).setEaseOutSine();
            selector.transform.position = new(room.x, room.y, -1);
        }

        void OpenDatasetDialog()
        {
            var path = Path.Combine(Application.persistentDataPath, "dataset");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select dataset (.csv)", path, "csv", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
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

        void SetFeatureDataText(MockRoom room)
        {
            featureDataTmp.text = @$"<b>Classification Status</b>
{room.ClassificationStatus}

<b>Enemies</b>
Fire weak:  {room.Stats.GetStat(StatKey.EnemyCountFire).Value}
Beam weak: {room.Stats.GetStat(StatKey.EnemyCountBeam).Value}
Wave weak: {room.Stats.GetStat(StatKey.EnemyCountWave).Value}
Total: {room.Stats.TotalEnemyCount}

<b>Obstacles</b>
Fire obstacles: {room.Stats.GetStat(StatKey.ObstacleCountFire).Value}
Beam obstacles: {room.Stats.GetStat(StatKey.ObstacleCountBeam).Value}
Wave obstacles: {room.Stats.GetStat(StatKey.ObstacleCountWave).Value}
Total: {room.Stats.TotalObstacleCount}";
        }

        void ResetFeatureDataText()
        {
            featureDataTmp.text = @$"<b>Classification Status</b>
-

<b>Enemies</b>
Fire weak: -
Beam weak: -
Wave weak: -
Total: -

<b>Obstacles</b>
Fire obstacles: -
Beam obstacles: -
Wave obstacles: -
Total: -";
        }
    }
}