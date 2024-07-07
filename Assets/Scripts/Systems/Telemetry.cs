using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RL.Systems
{
    [Serializable]
    public class Stat
    {
        public string Name;
        int _value;

        public event EventHandler OnValueChanged;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public void Increment()
        {
            _value++;
            OnValueChanged?.Invoke(this, new());
        }

        public Stat(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    public class StatCollection
    {
        Dictionary<string, Stat> _statList = new();
        public Dictionary<string, Stat> Stats => _statList;

        public StatCollection(string[] stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public Stat this[string name]
        {
            get
            {
                if (_statList.ContainsKey(name))
                {
                    return _statList[name];
                } else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public bool TryGetStat(string name, out Stat stat)
        {
            if (_statList.ContainsKey(name))
            {
                stat = _statList[name];
                return true;
            } else
            {
                stat = default;
                return false;
            }
        }

        internal void Reset()
        {
            foreach (var stat in _statList)
            {
                stat.Value.Value = 0;
            }
        }
    }

    public class Telemetry : MonoBehaviour
    {
        StatCollection _playerStats;
        public StatCollection PlayerStats => _playerStats;
        StatCollection _gameStats;
        public StatCollection GameStats => _gameStats;
        StatCollection _roomStats;
        public StatCollection RoomStats => _roomStats;

        Dictionary<string, TextMeshProUGUI> statTexts = new();

        [SerializeField] GameObject playerStatsContainer;
        [SerializeField] GameObject gameStatsContainer;
        [SerializeField] GameObject roomStatsContainer;

        internal void Initialize()
        {
            Debug.Log("Initializing telemetry...");
            InitializePlayerStats();
            InitializeGameStats();
            InitializeRoomStats();
        }

        void InitializePlayerStats()
        {
            string[] playerStats = {
                "useCountFire",
                "useCountBeam",
                "useCountWave",
                "hitCountFire",
                "hitCountBeam",
                "hitCountWave",
                "hitsTaken",
            };
            _playerStats = new(playerStats);

            foreach (var stat in _playerStats.Stats)
            {
                var go = new GameObject("Player Stat");
                go.transform.SetParent(playerStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Key}: {stat.Value.Value}";
                stat.Value.OnValueChanged += OnValueChangedCallback;
                statTexts[stat.Key] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerStatsContainer.transform as RectTransform);
        }

        void InitializeGameStats()
        {
            string[] gameStats = {
                "enemyAttackCount",
            };
            _gameStats = new(gameStats);

            foreach (var stat in _gameStats.Stats)
            {
                var go = new GameObject("Game Stat");
                go.transform.SetParent(gameStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Key}: {stat.Value.Value}";
                stat.Value.OnValueChanged += OnValueChangedCallback;
                statTexts[stat.Key] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameStatsContainer.transform as RectTransform);
        }

        void InitializeRoomStats()
        {
            string[] roomStats = {
                "enemyCountFire",
                "enemyCountBeam",
                "enemyCountWave",
                "obstacleCountFire",
                "obstacleCountBeam",
                "obstacleCountWave",
            };
            _roomStats = new(roomStats);
            
            foreach (var stat in _roomStats.Stats)
            {
                var go = new GameObject("Room Stat");
                go.transform.SetParent(roomStatsContainer.transform);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{stat.Key}: {stat.Value.Value}";
                stat.Value.OnValueChanged += OnValueChangedCallback;
                statTexts[stat.Key] = tmp;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(roomStatsContainer.transform as RectTransform);
        }

        void OnValueChangedCallback(object sender, EventArgs e)
        {
            var stat = sender as Stat;
            statTexts[stat.Name].text = $"{stat.Name}: {stat.Value}";
        }

        public Dictionary<string, Stat> RetrievePlayerModel()
        {
            return _playerStats.Stats;
        }
    }
}