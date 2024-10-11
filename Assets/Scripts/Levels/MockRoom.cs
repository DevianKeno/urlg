using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

using RL.Classifiers;
using RL.Telemetry;
using static RL.Generator.Generator.Map;

namespace RL.CellularAutomata
{
    public enum RecolorType { ENEMY, OBSTACLE, BOTH }

    /// <summary>
    /// Room component.
    /// </summary>
    public class MockRoom : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int Coordinates { get; set; }
        public int x => Coordinates.x;
        public int y => Coordinates.y;
        /// Just to note if a start/end room
        public bool IsSpecial => IsStartRoom || IsEndRoom;
        public Status ClassificationStatus = Status.None;
        // public RoomType Type;
        FeatureParameters features;
        public FeatureParameters Features => features;
        RoomStatCollection roomStats;
        public RoomStatCollection Stats => roomStats;
        Dictionary<Cardinal, MockRoom> neighbors = new();
        public Dictionary<Cardinal, MockRoom> Neighbors => neighbors;
        Color enemyAlignmentColor;
        public Color EnemyAlignmentColor => enemyAlignmentColor;
        Color obsAlignmentColor;
        public Color ObstacleAlignmentColor => obsAlignmentColor;

        Color _originalColor;

        public event Action<MockRoom> OnClick;

        public bool IsStartRoom;
        public bool IsEndRoom;
        
        [Header("Doors")]
        public bool North;
        public bool South;
        public bool East;
        public bool West;

        [SerializeField] GameObject doorNorth;
        [SerializeField] GameObject doorSouth;
        [SerializeField] GameObject doorEast;
        [SerializeField] GameObject doorWest;

        [Header("Components")]
        [SerializeField] SpriteRenderer spriteRenderer;
        
        [Header("Colors")]
        public Color FireAlignmentColor = Color.red;
        public Color BeamAlignmentColor = Color.green;
        public Color WaveAlignmentColor = Color.blue;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                doorNorth?.SetActive(North);
                doorSouth?.SetActive(South);
                doorEast?.SetActive(East);
                doorWest?.SetActive(West);
            }
        }

        void Start()
        {
            _originalColor = spriteRenderer.color;
        }
        

        #region Public methods

        public void GenerateFeatures(FeatureParameters options)
        {
            this.features = options;
            RecalculateStats();
            CalculateAlignmentColor();
        }
        
        public void Recolor(RecolorType type = RecolorType.BOTH)
        {
            if (IsSpecial || Stats == null) return;

            CalculateAlignmentColor();
            if (type == RecolorType.ENEMY)
            {
                SetColorEnemyAligned();
            } else if (type == RecolorType.OBSTACLE)
            {
                SetColorObstacleAligned();
            } else if (type == RecolorType.BOTH)
            {
                SetColorBothAligned();
            }
        }

        public void Featurize(RoomStatCollection roomStats)
        {
            this.roomStats = roomStats;
        }

        public void RecalculateStats()
        {
            roomStats = new(Telemetry.Telemetry.RoomStatsKeys);
            roomStats.GetStat(StatKey.EnemyCountFire).Value = features.EnemyCountFire;
            roomStats.GetStat(StatKey.EnemyCountBeam).Value = features.EnemyCountBeam;
            roomStats.GetStat(StatKey.EnemyCountWave).Value = features.EnemyCountWave;
            roomStats.GetStat(StatKey.ObstacleCountFire).Value = features.ObstacleCountFire;
            roomStats.GetStat(StatKey.ObstacleCountBeam).Value = features.ObstacleCountBeam;
            roomStats.GetStat(StatKey.ObstacleCountWave).Value = features.ObstacleCountWave;
        }

        public void SetColorEnemyAligned()
        {
            spriteRenderer.color = enemyAlignmentColor;
        }

        public void SetColorObstacleAligned()
        {
            spriteRenderer.color = obsAlignmentColor;
        }

        public void SetColorBothAligned()
        {
            spriteRenderer.color = enemyAlignmentColor + obsAlignmentColor;
        }

        public void CalculateAlignmentColor()
        {
            var ef = Stats.GetStat(StatKey.EnemyCountFire).Value;
            var eb = Stats.GetStat(StatKey.EnemyCountBeam).Value;
            var ew = Stats.GetStat(StatKey.EnemyCountWave).Value;
            if (ef > eb) enemyAlignmentColor = FireAlignmentColor;
            else if (eb > ew) enemyAlignmentColor = BeamAlignmentColor;
            else if (ew > ef) enemyAlignmentColor = WaveAlignmentColor;

            var of = Stats.GetStat(StatKey.ObstacleCountFire).Value;
            var ob = Stats.GetStat(StatKey.ObstacleCountBeam).Value;
            var ow = Stats.GetStat(StatKey.ObstacleCountWave).Value;
            if (of > ob) obsAlignmentColor = FireAlignmentColor;
            else if (ob > ow) obsAlignmentColor = BeamAlignmentColor;
            else if (ow > of) obsAlignmentColor = WaveAlignmentColor;
        }

        public void CalculateAlignmentColorMixed()
        {
            Stat stat;
            /// calculate enemies
            int totalEnemies = 0; 
            if (roomStats.TryGetStat(StatKey.EnemyCountFire, out stat) && stat.Value != 0)
            {
                totalEnemies += stat.Value;
            }
            if (roomStats.TryGetStat(StatKey.EnemyCountBeam, out stat) && stat.Value != 0)
            {
                totalEnemies += stat.Value;
            }
            if (roomStats.TryGetStat(StatKey.EnemyCountWave, out stat) && stat.Value != 0)
            {
                totalEnemies += stat.Value;
            }
            float enemyRatioFire = (float) stat.Value / (float) totalEnemies;
            float enemyRatioBeam = (float) stat.Value / (float) totalEnemies;
            float enemyRatioWave = (float) stat.Value / (float) totalEnemies;
            enemyAlignmentColor = (FireAlignmentColor * enemyRatioFire) + (BeamAlignmentColor * enemyRatioBeam) + (WaveAlignmentColor * enemyRatioWave);
            enemyAlignmentColor.a = 1f;

            /// calculate obstacles
            int totalObstacles = 0; 
            if (roomStats.TryGetStat(StatKey.ObstacleCountFire, out stat) && stat.Value != 0)
            {
                totalObstacles += stat.Value;
            }
            if (roomStats.TryGetStat(StatKey.ObstacleCountBeam, out stat) && stat.Value != 0)
            {
                totalObstacles += stat.Value;
            }
            if (roomStats.TryGetStat(StatKey.ObstacleCountWave, out stat) && stat.Value != 0)
            {
                totalObstacles += stat.Value;
            }
            float obsRatioFire = (float) stat.Value / (float) totalObstacles;
            float obsRatioBeam = stat.Value / (float)totalObstacles;
            float obsRatioWave = (float) stat.Value / (float) totalObstacles;
            obsAlignmentColor = (FireAlignmentColor * obsRatioFire) + (BeamAlignmentColor * obsRatioBeam) + (WaveAlignmentColor * obsRatioWave);
            obsAlignmentColor.a = 1f;
        }

        /// <summary>
        /// Connects the doorways of this Room to the given Room.
        /// </summary>
        public void ConnectDoorways(MockRoom other)
        {
            if (other != null)
            {
                Neighbors.Add(DirectionTo(other), other);
                other.Neighbors.Add(other.DirectionTo(this), this);
            }
        }

        /// <summary>
        /// Gets the cardinal direction of the given Room <b>from</b> this Room.
        /// </summary>
        public Cardinal DirectionTo(MockRoom other)
        {
            int dx = other.x - x;
            int dy = other.y - y;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return dx > 0 ? Cardinal.East : Cardinal.West;
            return dy > 0 ? Cardinal.North : Cardinal.South;
        }

        public void ToggleDoorway(Cardinal cardinal, bool isOpen)
        {
            switch (cardinal)
            {
                case Cardinal.North:
                {
                    North = isOpen;
                    doorNorth?.SetActive(isOpen);
                    break;
                }
                case Cardinal.South:
                {
                    South = isOpen;
                    doorSouth?.SetActive(isOpen);
                    break;
                }
                case Cardinal.East:
                {
                    East = isOpen;
                    doorEast?.SetActive(isOpen);
                    break;
                }
                case Cardinal.West:
                {
                    West = isOpen;
                    doorWest?.SetActive(isOpen);
                    break;
                }
            }
        }

        public MockRoom GetNeighbor(Cardinal cardinal)
        {
            neighbors.TryGetValue(cardinal, out MockRoom neighbor);
            return neighbor;
        }

        public void OnMouseEnter()
        {
            LeanTween.cancel(gameObject);

            var color = spriteRenderer.color;
            color.a = 0.66f;
            spriteRenderer.color = color;
        }

        public void OnMouseDown()
        {
            var color = spriteRenderer.color;
            color.a = 0.33f;
            spriteRenderer.color = color;
            OnClick?.Invoke(this);
            Invoke(nameof(ResetColor), 0.1f);
        }

        public void OnMouseExit()
        {
            ResetColor();
        }

        #endregion    


        void ResetColor()
        {
            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}