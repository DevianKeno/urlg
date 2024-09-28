using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace URLG.Telemetry
{
    public class Telemetry : MonoBehaviour
    {
        public static string[] PlayerStatsValues = {
            "UseCountFire",
            "UseCountBeam",
            "UseCountWave",
            "HitCountFire",
            "HitCountBeam",
            "HitCountWave",
            "HitsTaken",
        };
        public static string[] RoomStatsValues = {
            "EnemyCountFire",
            "EnemyCountBeam",
            "EnemyCountWave",
            "ObstacleCountFire",
            "ObstacleCountBeam",
            "ObstacleCountWave",
        };
        public static string[] GameStatsValues = {
            "EnemyAttackCount"
        };

        StatCollection _playerStats;
        public StatCollection PlayerStats => _playerStats;
        StatCollection _gameStats;
        public StatCollection GameStats => _gameStats;
        StatCollection _roomStats;
        public StatCollection RoomStats => _roomStats;

        public bool Display = true;

        Dictionary<string, TextMeshProUGUI> statTexts = new();

        [Header("Elements")]
        [SerializeField] GameObject telemetryContainer;
        [SerializeField] GameObject playerStatsContainer;
        [SerializeField] GameObject gameStatsContainer;
        [SerializeField] GameObject roomStatsContainer;


        #region Initializing methods

        internal void Initialize()
        {
            Debug.Log("Initializing telemetry...");

            InitializePlayerStats();
            InitializeGameStats();
            InitializeRoomStats();

            Debug.Log("Done initialize telemetry");
        }

        void InitializePlayerStats()
        {
            _playerStats = new(PlayerStatsValues);
            foreach (var stat in _playerStats.Stats)
            {
                var go = new GameObject("Player Stat");
                go.transform.SetParent(playerStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Name}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.Name] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerStatsContainer.transform as RectTransform);
        }

        void InitializeGameStats()
        {
            _gameStats = new(GameStatsValues);
            foreach (var stat in _gameStats.Stats)
            {
                var go = new GameObject("Game Stat");
                go.transform.SetParent(gameStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Name}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.Name] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameStatsContainer.transform as RectTransform);
        }

        void InitializeRoomStats()
        {
            _roomStats = new(RoomStatsValues);
            foreach (var stat in _roomStats.Stats)
            {
                var go = new GameObject("Room Stat");
                go.transform.SetParent(roomStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Name}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.Name] = tmp;
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
            statTexts[stat.Name].text = $"{stat.Name}: {stat.Value}";
        }

        #endregion

        
        #region Public methods

        public void SaveToJson()
        {
            var sd = new URLGSaveData()
            {
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                PlayerStats = PlayerStats.SaveToJson(),
                RoomStats = RoomStats.SaveToJson(),
                GameStats = GameStats.SaveToJson(),
            };

            Game.Files.SaveDataJson(sd);
        }

        #endregion
    }
}