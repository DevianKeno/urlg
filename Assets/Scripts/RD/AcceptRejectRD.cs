/*
Program Title: Accept and Reject [Sampling Algorithm] (Research and Development)
Date written: October 4, 2024
Date revised: October 16, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component aims to gather the gameplay characteristics of the player as statistics.

Data Structures:
    List: 
*/

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using RL.Graphs;
using RL.CellularAutomata;
using RL.Classifiers;
using RL.Telemetry;
using RL.UI;
using RL.RD.UI;

namespace RL.RD
{
    public class AcceptRejectRD : MonoBehaviour
    {
        public const int MaxBulkGenerationTimes = 1000;
        public const string AcceptedMessage = @"The generated room matches the player's preferences,
therefore is <b>accepted</b>.";
        public const string RejectedMessage = @"The generated room falls out of the player's preferences,
therefore is <b>rejected</b>.";

        public MockRoom Room = null;
        [Range(0f, 1f)]
        public float AcceptanceThreshold = 0f;
        public bool NormalizeValues => normalizeToggle != null ? normalizeToggle.isOn : false;
        /// <summary>
        /// The current state of Player Stats.
        /// </summary>
        PlayerStatCollection playerStats;
        /// <summary>
        /// The current state of Room Stats.
        /// </summary>
        RoomStatCollection roomStats;
        ARResult _previousResult;

        [SerializeField] RDTelemetryUI playerTelemetryUI;
        [SerializeField] RDTelemetryUI roomTelemetryUI;
        [SerializeField] ConfusionMatrixHandler confusionMatrixHandler;

        [Header("Bulk Gen Settings")]
        int highestIteration = 0;
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
        [SerializeField] Button setValuesBtn;
        [SerializeField] Button bulkGenBtn;
        [SerializeField] Button generateRoomBtn;
        [SerializeField] Toggle normalizeToggle;
        [SerializeField] Button likedBtn;
        [SerializeField] Button dislikedBtn;
        
        [Header("Texts")]
        [SerializeField] TextMeshProUGUI featureDataContentTmp;
        [SerializeField] TextMeshProUGUI highestIterationsTmp;
        [SerializeField] TextMeshProUGUI iterationsTmp;
        [SerializeField] TextMeshProUGUI timeTmp;
        [SerializeField] TextMeshProUGUI acceptedTmp;
        [SerializeField] TextMeshProUGUI rejectedTmp;
        [SerializeField] TextMeshProUGUI messageTmp;

        [Space]
        
        [SerializeField] GameObject mockRoomPrefab;

        void Awake()
        {
            setValuesBtn.onClick.AddListener(SetPlayerValues);
            generateRoomBtn.onClick.AddListener(GenerateRoom);
            bulkGenBtn.onClick.AddListener(BulkGenerateUntilStatus);

            likedBtn.onClick.AddListener(LikeFeatureSet);
            dislikedBtn.onClick.AddListener(DislikeFeatureSet);
        }

        void Start()
        {
            ClearAllPoints();
            ResetARStatus();
            bulkGenTimer = new();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ClassifyAR();
            }
        }


        #region Public methods

        /// <summary>
        /// Checks the current player state and room stat states and classifies it using Accept-Reject algorithm.
        /// </summary>
        public void ClassifyAR()
        {
            if (playerStats == null || roomStats == null)
            {
                Debug.LogError($"Neither player nor room stats can be null for classification");
                return;
            }

            var result = ARClassifier.Classify(playerStats, roomStats, AcceptanceThreshold, normalizeToggle.isOn);
            if (result.Status == Status.Accepted)
            {
                SetARAccepted();
            }
            else if (result.Status == Status.Rejected)
            {
                SetARRejected();
            }
            _previousResult = result;
        }

        bool _continueBulkGeneration;

        public void BulkGenerateUntilStatus()
        {
            ResetARStatus();
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
            SetARRejected();
            bulkGenTimer.Start();


            int iterations = 0;
            for (int i = 0; i < MaxBulkGenerationTimes; i++)
            {
                roomTelemetryUI.RandomizeRoomFeatures();
                SetRoomValues();
                _previousResult = ARClassifier.Classify(playerStats, roomStats, AcceptanceThreshold, normalizeToggle.isOn);                
                iterationsTmp.text = message + i;
                iterations = i;
                if (!_continueBulkGeneration ||_previousResult.Status == BulkGenTarget) break;
                if (VisualizeBulkGen) yield return new WaitForSeconds(BulkGenDelay);
            }

            bulkGenTimer.Stop();
            timeTmp.text = $"Time: {bulkGenTimer.ElapsedMilliseconds} ms";
            highestIteration = System.Math.Max(highestIteration, iterations);
            highestIterationsTmp.text = $"Highest iterations: {highestIteration}"; 
            SetARAccepted();
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
            
            if (roomStats != null)
            {
                var dodgeRating = Evaluate.Player.DodgeRating(
                    playerStats[StatKey.HitsTaken].Value,
                    roomStats[StatKey.EnemyAttackCount].Value);
                skillGraph.SetBoundsY(0f, (float) dodgeRating);
            }
        }

        public void RandomizeHitsTaken()
        {
            var attackCount = roomTelemetryUI.GetEntry(StatKey.EnemyAttackCount).Value;
            var hitsTaken = UnityEngine.Random.Range(0, attackCount); /// UNSEEDED
            
            playerTelemetryUI.GetEntry(StatKey.HitsTaken).Value = hitsTaken;
            playerStats.GetStat(StatKey.HitsTaken).Value = hitsTaken;
        }

        /// <summary>
        /// Clamps the stat HitsTaken between its minimum and maximum possible values.
        /// </summary>
        public void ClampHitsTaken()
        {
            var hitsTaken = playerTelemetryUI.GetEntry(StatKey.HitsTaken).Value;
            hitsTaken = System.Math.Clamp(hitsTaken, 0, roomTelemetryUI.GetEntry(StatKey.EnemyAttackCount).Value);
            
            playerTelemetryUI.GetEntry(StatKey.HitsTaken).Value = hitsTaken;
            playerStats.GetStat(StatKey.HitsTaken).Value = hitsTaken;
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
            var difficultyPref = Evaluate.Room.Difficulty(roomStats, Game.MaxEnemiesPerRoom);

            ClearAllPoints();
            fireGraph.PlotPoint((float) firePref);
            beamGraph.PlotPoint((float) beamPref);
            waveGraph.PlotPoint((float) wavePref);
            skillGraph.PlotPoint((float) difficultyPref);
        }

        public void SetSkillPref()
        {
            if (playerStats == null || roomStats == null) return;

            var dodgeRating = Evaluate.Player.DodgeRating(playerStats[StatKey.HitsTaken].Value, roomStats[StatKey.EnemyAttackCount].Value);
            skillGraph.SetBoundsY(0f, (float) dodgeRating);
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
    
        string hexGray = "4B4B4B";
        string hexGreen = "40D945";
        string hexRed = "F62B2B";

        public void SetARAccepted()
        {
            acceptedTmp.text = $"<color=#{hexGreen}>Accepted";
            rejectedTmp.text = $"<color=#{hexGray}>Rejected";
            messageTmp.text = AcceptedMessage;
        }

        public void SetARRejected()
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
            confusionMatrixHandler.Reset();
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

        #region Helpers

        void GenerateRoom() /// random
        {
            if (Room != null) Destroy(Room);
            Room = CreateRoom();
            FeaturizeRandom(Room);
            PlotPoints();
        }

        void ClearAllPoints()
        {
            fireGraph.RemovePoints();
            beamGraph.RemovePoints();
            waveGraph.RemovePoints();
        }

        MockRoom CreateRoom()
        {
            GameObject go;
#if UNITY_EDITOR
            go = (GameObject) PrefabUtility.InstantiatePrefab(mockRoomPrefab);
#else
            go = (GameObject) Instantiate(mockRoomPrefab);
#endif
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            var mockRoom = go.GetComponent<MockRoom>();
            return mockRoom;
        }

        void FeaturizeRandom(MockRoom room)
        {
            if (room == null) return;
            
            var settings = new FeatureParametersSettings()
            {
                MaxEnemyCount = 20,
                MaxObstacleCount = 6,
            };
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var generatedFeatureParameters = GaussianNaiveBayes.GenerateFeatureOptionsRandom(settings, seed);
            room.GenerateFeatures(generatedFeatureParameters);
        }

        #endregion
    }
}