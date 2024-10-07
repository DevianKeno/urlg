using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using URLG.Telemetry;
using static URLG.Generator.Generator.Map;

namespace URLG.CellularAutomata
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
        public bool IsSpecial { get; set; } = false;
        // public RoomType Type;
        FeatureParameters features;
        public FeatureParameters Features => features;

        Dictionary<Cardinal, MockRoom> neighbors = new();
        public Dictionary<Cardinal, MockRoom> Neighbors => neighbors;
        StatCollection stats;
        public StatCollection Stats => stats;
        Color enemyAlignmentColor;
        public Color EnemyAlignmentColor => enemyAlignmentColor;
        Color obsAlignmentColor;
        public Color ObstacleAlignmentColor => obsAlignmentColor;

        Color _originalColor;

        public event Action<MockRoom> OnClick;

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
            if (IsSpecial) return;

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

        public void RecalculateStats()
        {
            stats = new(Telemetry.Telemetry.RoomStatsValues);
            stats.GetStat("EnemyCountFire").Value = features.EnemyCountFire;
            stats.GetStat("EnemyCountBeam").Value = features.EnemyCountBeam;
            stats.GetStat("EnemyCountWave").Value = features.EnemyCountWave;
            stats.GetStat("ObstacleCountFire").Value = features.ObstacleCountFire;
            stats.GetStat("ObstacleCountBeam").Value = features.ObstacleCountBeam;
            stats.GetStat("ObstacleCountWave").Value = features.ObstacleCountWave;
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
            var ef = Stats.GetStat("EnemyCountFire").Value;
            var eb = Stats.GetStat("EnemyCountBeam").Value;
            var ew = Stats.GetStat("EnemyCountWave").Value;
            if (ef > eb) enemyAlignmentColor = FireAlignmentColor;
            else if (eb > ew) enemyAlignmentColor = BeamAlignmentColor;
            else if (ew > ef) enemyAlignmentColor = WaveAlignmentColor;

            var of = Stats.GetStat("ObstacleCountFire").Value;
            var ob = Stats.GetStat("ObstacleCountBeam").Value;
            var ow = Stats.GetStat("ObstacleCountWave").Value;
            if (of > ob) obsAlignmentColor = FireAlignmentColor;
            else if (ob > ow) obsAlignmentColor = BeamAlignmentColor;
            else if (ow > of) obsAlignmentColor = WaveAlignmentColor;
        }

        public void CalculateAlignmentColorMixed()
        {
            Stat stat;
            /// calculate enemies
            int totalEnemies = 0; 
            if (stats.TryGetStat("EnemyCountFire", out stat) && stat.Value != 0)
            {
                totalEnemies += stat.Value;
            }
            if (stats.TryGetStat("EnemyCountBeam", out stat) && stat.Value != 0)
            {
                totalEnemies += stat.Value;
            }
            if (stats.TryGetStat("EnemyCountWave", out stat) && stat.Value != 0)
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
            if (stats.TryGetStat("ObstacleCountFire", out stat) && stat.Value != 0)
            {
                totalObstacles += stat.Value;
            }
            if (stats.TryGetStat("ObstacleCountBeam", out stat) && stat.Value != 0)
            {
                totalObstacles += stat.Value;
            }
            if (stats.TryGetStat("ObstacleCountWave", out stat) && stat.Value != 0)
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