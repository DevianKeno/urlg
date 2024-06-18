using System;
using System.Collections.Generic;
using RL.Levels;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RL.Generator
{
    [Serializable]
    public struct GeneratorParams
    {
        public int RoomWidth;
        public int RoomHeight;
    }

    public class Generator : MonoBehaviour
    {
        [SerializeField] List<TileData> tiles = new();
        Dictionary<string, TileData> tilesDict = new();
        public GeneratorParams GeneratorParams = new();

        [Header("Create")]
        public string TileId;
        public Vector2Int Cell;

        [SerializeField] GameObject tilePrefab;
        [SerializeField] GameObject roomPrefab;

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            foreach (var t in tiles)
            {
                if (!tilesDict.ContainsKey(t.Id))
                {
                    tilesDict[t.Id] = t;
                }
            }
        }

        public Tile CreateTile(string id, Vector2Int coords)
        {
            Tile t = null;
            if (tilesDict.ContainsKey(id))
            {
                var go = Instantiate(tilePrefab);
                t = go.GetComponent<Tile>();
                t.SetData(tilesDict[id]);
                t.CoordinateToPosition(coords);
                t.Initialize();
            }            
            return t;
        }

        public void CreateRoom(GeneratorParams p)
        {
            var go = Instantiate(roomPrefab);
            go.name = "New Room";
            var room = go.GetComponent<Room>();


            // var go = new GameObject($"Room");
            // var room = go.AddComponent<Room>();

            // for (int x = 0; x < p.RoomWidth; x++)
            // {
            //     for (int y = 0; y < p.RoomHeight; y++)
            //     {
            //         go = Instantiate(floorPrefab, go.transform);
            //         if (go.TryGetComponent(out Tile tile))
            //         {
            //             tile.SetCoordinates(new Vector2Int(x, y));
            //             room.Tiles.Add(tile);
            //         }
            //     }
            // }
            // CreateWalls(p.RoomWidth, p.RoomHeight);
            // room.Initialize(GenerateRoomContent());

        }
        
        RoomData GenerateRoomContent()
        {
            return new RoomData
            {
                /// Enemy count
                EnemyCountFire = Random.Range(1, 10),
                EnemyCountBeam = Random.Range(1, 10),
                EnemyCountWave = Random.Range(1, 10),
                /// Obstacle count
                ObstacleCountFire = Random.Range(1, 10),
                ObstacleCountBeam = Random.Range(1, 10),
                ObstacleCountWave = Random.Range(1, 10)
            };
        }

        void CreateDoors()
        {
            
        }

        void CreateWalls(int width, int height)
        {
            for (int x = -1; x < width + 1; x++)
            {
                CreateWall(new Vector2Int(x, -1));
                CreateWall(new Vector2Int(x, height));
            }
            for (int y = 0; y < height; y++)
            {
                CreateWall(new Vector2Int(-1, y));
                CreateWall(new Vector2Int(width, y));
            }
        }

        void CreateWall(Vector2Int coordinates)
        {
            // GameObject go = Instantiate(wallPrefab, roomGO.transform);
            // if (go.TryGetComponent(out Tile tile))
            // {
            //     tile.SetCoordinates(coordinates);
            //     room.Tiles.Add(tile);
            // }
        }
    }
}
