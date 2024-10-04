using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
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
        };
        public static StatKey[] GameStatsKeys = {
            StatKey.EnemyAttackCount
        };

        PlayerStatCollection _playerStats;
        public PlayerStatCollection PlayerStats => _playerStats;
        StatCollection _gameStats;
        public StatCollection GameStats => _gameStats;
        RoomStatCollection _roomStats;
        public RoomStatCollection RoomStats => _roomStats;

        public bool Display = true;

        Dictionary<StatKey, TextMeshProUGUI> statTexts = new();

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
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerStatsContainer.transform as RectTransform);
        }

        void InitializeGameStats()
        {
            _gameStats = new(GameStatsKeys);
            foreach (var stat in _gameStats.Stats)
            {
                var go = new GameObject("Game Stats");
                go.transform.SetParent(gameStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.key}: {stat.Value}";

                stat.OnValueChanged += OnValueChangedCallback;

                statTexts[stat.key] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameStatsContainer.transform as RectTransform);
        }

        void InitializeRoomStats()
        {
            _roomStats = new(RoomStatsKeys);
            foreach (var stat in _roomStats.Stats)
            {
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