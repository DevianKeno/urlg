using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

using RL.Enemies;

using RL.Levels;
using Random = UnityEngine.Random;
using RL.Telemetry;

namespace RL.Generator
{
    public class Generator : MonoBehaviour
    {
        [Header("Creation Parameters")]
        public string TileId;
        public Vector2Int Cell;

        [Header("Enemy Variance")]
        public int minDeer = 0;
        public int maxDeer = 7;
        public int minBeamWeak = 0;
        public int maxBeamWeak = 7;
        public int minSalaman = 0;
        public int maxSalaman = 7;

        [Header("Obstacle Variance")]
        public int minWaveObs = 0;
        public int maxWaveObs  = 7;
        public int minCrate = 0;
        public int maxCrate = 7;
        public int minBeamObs = 0;
        public int maxBeamObs = 7;

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

            var salamanCount = UnityEngine.Random.Range(minSalaman, maxSalaman);

            for (int i = 0; i < salamanCount; i++ )
            {
                Instantiate(salamanPrefab, enemiesContainer.transform);
                Game.Telemetry.RoomStats[StatKey.EnemyCountWave].Increment();
            }
        }
        
        public RoomData CreateRandomizedContent()
        {
            return new RoomData
            {
                /// Enemy count
                EnemyCountFire = UnityEngine.Random.Range(minDeer, maxDeer),
                EnemyCountBeam = UnityEngine.Random.Range(minBeamObs, maxBeamObs),
                EnemyCountWave = UnityEngine.Random.Range(minSalaman, maxSalaman),

                /// Obstacle count
                ObstacleCountFire = UnityEngine.Random.Range(minCrate, maxCrate),
                ObstacleCountBeam = UnityEngine.Random.Range(minBeamObs, maxBeamObs),
                ObstacleCountWave = UnityEngine.Random.Range(minWaveObs, maxWaveObs),
            };
        }

        #endregion


        void CreateWall(Vector2Int coordinates)
        {
            // GameObject go = Instantiate(wallPrefab, roomGO.transform);
            // if (go.TryGetComponent(out Tile tile))
            // {
            //     tile.SetCoordinates(coordinates);
            //     room.Tiles.Add(tile);
            // }
        }

        public class Map
        {
            public enum Cardinal {
                North, South, East, West
            }

            Dictionary<Cardinal, GameObject> data = new();


            public void Generate()
            {
                
            }
        }
    }
}
