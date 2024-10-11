using System;
using System.Collections.Generic;

using UnityEngine;

using RL.Entities;
using RL.Levels;

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
