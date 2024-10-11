using System;
using System.Collections.Generic;

using UnityEngine;

using RL.Entities;
using RL.Levels;
using RL.CellularAutomata;
using RL.Classifiers;
using RL.Telemetry;
using RL.UI;

namespace RL.Generator
{
    public class Generator : MonoBehaviour
    {
        [Header("Creation Parameters")]
        public string TileId;
        public Vector2Int Cell;

        [SerializeField] GameObject enemiesContainer;
        

        #region Prefabs
        [SerializeField] GameObject tilePrefab;
        [SerializeField] GameObject roomPrefab;
        [SerializeField] GameObject salamanPrefab;

        #endregion


        #region Public methods
        
        public Room InstantiateRoom(int x, int y)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Rooms/ROOM_BASE");
            var go = Instantiate(prefab, transform);
            var room = go.GetComponent<Room>();
            
            go.transform.SetPositionAndRotation(
                new Vector3(
                    x * 22,
                    (y * 16) - (y),
                    0f),
                Quaternion.identity
            );
            
            return room;
        }
        
        public RoomStatCollection GenerateRoomStats(FeaturizeOptions options)
        {
            if (options.Algorithm == PCGAlgorithm.AcceptReject)
            {
                return GenerateFeaturesAR(options);
            }
            else if (options.Algorithm == PCGAlgorithm.GaussianNaiveBayes)
            {
                return GenerateFeaturesGNB(options);
            }

            return new(Telemetry.Telemetry.RoomStatsKeys);
        }

        const int MaxAttempts = 256;
        RoomStatCollection GenerateFeaturesAR(FeaturizeOptions options)
        {   
            RoomStatCollection roomStats;
            ARResult previousResult;

            int attempts = 0;
            do
            {
                roomStats = RDTelemetryUI.ConstructRoomRandom(
                    options.MaxEnemyCount,
                    options.MaxObstacleCount);

                previousResult = ARClassifier.Classify(options.PlayerStats, roomStats, 0.2f, normalized: true);

                if (previousResult.Status == options.TargetStatus) break;
                attempts++;
            }
            while (attempts < MaxAttempts);

            return roomStats;
        }

        RoomStatCollection GenerateFeaturesGNB(FeaturizeOptions options)
        {
            RoomStatCollection roomStats;
            GNBResult previousResult;

            int attempts = 0;
            do
            {
                roomStats = RDTelemetryUI.ConstructRoomRandom(
                    options.MaxEnemyCount,
                    options.MaxObstacleCount);

                previousResult = GaussianNaiveBayes.Instance.ClassifyRoom(options.PlayerStats, roomStats);  
                if (previousResult.Status == options.TargetStatus) break;
                attempts++;
            }
            while (attempts < MaxAttempts);

            return roomStats;
        }

        public void RegenerateEnemies()
        {
            Game.Telemetry.RoomStats.Reset();

            if (enemiesContainer.transform.childCount > 0)
            {
                foreach (Transform c in enemiesContainer.transform)
                {
                    if (c.TryGetComponent<Enemy>(out var enemy))
                    {
                        Destroy(enemy.gameObject);
                    }
                }
            }

            // var salamanCount = UnityEngine.Random.Range(minSalaman, maxSalaman);

            // for (int i = 0; i < salamanCount; i++ )
            // {
            //     Instantiate(salamanPrefab, enemiesContainer.transform);
            //     Game.Telemetry.RoomStats[StatKey.EnemyCountWave].Increment();
            // }
        }
        
        #endregion

        public class Map
        {
            public enum Cardinal {
                North, South, East, West
            }
        }
    }
}
