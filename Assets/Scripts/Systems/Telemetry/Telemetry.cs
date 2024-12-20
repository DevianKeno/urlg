using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using RL.Classifiers;
using UnityEngine.SceneManagement;

namespace RL.Telemetry
{
    public class Telemetry : MonoBehaviour
    {
        public static StatKey[] PlayerStatsKeys = {
            StatKey.UseCountFire,
            StatKey.UseCountBeam,
            StatKey.UseCountWave,
            StatKey.HitCountFire,
            StatKey.HitCountBeam,
            StatKey.HitCountWave,
            StatKey.HitsTaken,
        };
        public static StatKey[] RoomStatsKeys = {
            StatKey.EnemyCountFire,
            StatKey.EnemyCountBeam,
            StatKey.EnemyCountWave,
            StatKey.ObstacleCountFire,
            StatKey.ObstacleCountBeam,
            StatKey.ObstacleCountWave,
            StatKey.EnemyAttackCount,
        };
        
        public bool IsInitialized { get; private set; }
        public bool Display = true;
        public bool IsVisible { get; private set; }

        PlayerStatCollection _playerStats;
        public PlayerStatCollection PlayerStats => _playerStats;
        StatCollection _gameStats;
        public StatCollection GameStats => _gameStats;
        RoomStatCollection _currentRoomStats;
        public RoomStatCollection RoomStats => _currentRoomStats;

        int totalHitsTaken;
        int totalEnemyAttackCount;

        List<DataEntry> dataEntries = new();

        [Header("Elements")]
        Dictionary<StatKey, TextMeshProUGUI> statTexts = new();
        [SerializeField] GameObject telemetryContainer;
        [SerializeField] GameObject playerStatsContainer;
        [SerializeField] GameObject roomStatsContainer;


        #region Initializing methods

        void Start()
        {
            if (Display)
            {
                telemetryContainer.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (IsVisible)
                {
                    HideTelemetry();
                }
                else
                {
                    ShowTelemetry();
                }
            }
        }

        internal void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            
            Debug.Log("Initializing telemetry...");

            InitializePlayerStats();
            InitializeRoomStats();

            Debug.Log("Done initialize telemetry");
            HideTelemetry();
        }

        void InitializePlayerStats()
        {
            _playerStats = new(PlayerStatsKeys);
            foreach (var stat in _playerStats.Stats)
            {
                var go = new GameObject("Player Stats");
                go.transform.SetParent(playerStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.key}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.key] = tmp;
            }
            totalHitsTaken = 0;
            totalEnemyAttackCount = 0;
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerStatsContainer.transform as RectTransform);
        }

        void InitializeRoomStats()
        {
            _currentRoomStats = new(RoomStatsKeys);
            foreach (var stat in _currentRoomStats.Stats)
            {
                // if (stat.key == StatKey.EnemyAttackCount) continue;

                var go = new GameObject("Room Stat");
                go.transform.SetParent(roomStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.key}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.key] = tmp;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(roomStatsContainer.transform as RectTransform);
        }

        #endregion

        
        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                telemetryContainer.gameObject.SetActive(Display);
            }
        }


        #region Event callbacks

        void OnValueChangedCallback(object sender, EventArgs e)
        {
            var stat = (Stat) sender;
            statTexts[stat.key].text = $"{stat.key}: {stat.Value}";
        }

        #endregion

        
        #region Public methods

        public void ShowTelemetry()
        {
            if (SceneManager.GetActiveScene().name != "LEVEL") return;

            telemetryContainer.gameObject.SetActive(true);
            IsVisible = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerStatsContainer.transform as RectTransform);
        }

        public void HideTelemetry()
        {
            telemetryContainer.gameObject.SetActive(false);
            IsVisible = false;
        }

        public void SetRoomValues(RoomStatCollection stats)
        {
            if (stats == null) return;

            foreach (var stat in stats.Stats)
            {
                if (statTexts.TryGetValue(stat.key, out var tmp))
                {
                    tmp.text = $"{stat.key}: {stat.Value}";
                }
            }

            if (statTexts.TryGetValue(StatKey.HitsTaken, out var tmp1))
            {
                tmp1.text = $"{StatKey.HitsTaken}: {PlayerStats.GetStat(StatKey.HitsTaken).Value}";
            }
        }

        public void NewRoomStatInstance()
        {
            this._currentRoomStats = new(RoomStatsKeys);
        }

        public void SaveRoomStats(int groundTruth, RoomStatCollection roomStats, bool createNewAfter = true)
        {
            var playerStats = PlayerStats;
            var entry = new DataEntry()
            {
                // Classification = roomStats.Classification,
                Classification = 1,
                LevelNumber = Game.Main.currentLevel,
                GroundTruth = groundTruth,
                PlayerStats = playerStats.SaveToJson(),
                RoomStats = roomStats.SaveToJson(),
            };
            
            dataEntries.Add(entry);

            totalHitsTaken += PlayerStats.GetStat(StatKey.HitsTaken).Value;
            PlayerStats.GetStat(StatKey.HitsTaken).Value = 0;
            
            totalEnemyAttackCount += _currentRoomStats.GetStat(StatKey.EnemyAttackCount).Value;

            if (createNewAfter)
            {
               NewRoomStatInstance();
            }
        }

        public void SaveEntriesToJson()
        {
            if (dataEntries.Count == 0) return;
            
            var sd = new ResultsJsonData()
            {
                Algorithm = Game.Main.AlgorithmUsed.ToString(),
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Entries = dataEntries,
            };

            Game.Files.SaveDataJson(sd);
        }

        public void IncrementEnemyAttackCount()
        {
            var currentRoom = Game.Main.CurrentRoom;
            if (currentRoom == null) return;

            currentRoom.Stats[StatKey.EnemyAttackCount].Increment();
            
            if (statTexts.TryGetValue(StatKey.EnemyAttackCount, out var tmp))
            {
                tmp.text = $"{StatKey.EnemyAttackCount}: {currentRoom.Stats.GetStat(StatKey.EnemyAttackCount).Value}";
            }
        }

        #endregion
    }
}