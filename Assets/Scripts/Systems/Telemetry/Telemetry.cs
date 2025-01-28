/*

Program Title: Telemetry
Data written: June 19, 2024
Date revised: December 17, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Where the program fits in the general system design:
    Runs behind the scenes to measure the player's statistics.

Purpose:
    This component aims to gather the gameplay characteristics of the player as statistics.
    The recorded statistics are then used as data by the algorithms (AR and GNB), along with
    the likert-scale result to perform classifications and generate adaptive levels.

Control:
    1. Initialize()
        -> InitializePlayerStats()
        -> InitializeRoomStats

    This component is initialized at the start of the application and continues to run in the game's entire lifecycle.
    When in the level scene, every new room that the player enters resets the state of the current RoomStatCollection.
    The UI can be toggled with [Backspace].

Data Structures:
    StatKey[]: array used to store a specific collection of stat keys
    StatCollection: used to store statistics related to the entire game application
    PlayerStatCollection: used to store statistics specifically for the player
    RoomStatCollection: used to store statistics for the room the player is currently in
*/

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
        public bool IsVisible { get; private set; }

        PlayerStatCollection _playerStats;
        public PlayerStatCollection PlayerStats => _playerStats;
        StatCollection _gameStats;
        public StatCollection GameStats => _gameStats;
        RoomStatCollection _currentRoomStats;
        public RoomStatCollection RoomStats => _currentRoomStats;

        int totalHitsTaken;
        int totalEnemyAttackCount;
        int _deathCount;

        List<DataEntry> dataEntries = new();

        [SerializeField] bool display = true;

        [Header("Elements")]
        Dictionary<StatKey, TextMeshProUGUI> statTexts = new();
        [SerializeField] GameObject telemetryContainer;
        [SerializeField] GameObject playerStatsContainer;
        [SerializeField] GameObject roomStatsContainer;


        #region Initializing methods

        void Start()
        {
            if (display)
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
            _deathCount = 0;
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
                telemetryContainer.gameObject.SetActive(display);
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
                DeathCount = _deathCount,
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

        public void IncrementDeathCount()
        {
            _deathCount++;
        }

        #endregion
    }
}