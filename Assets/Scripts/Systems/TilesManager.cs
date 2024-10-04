using UnityEngine;
using RL.Levels;
using System.Collections.Generic;

namespace RL.Systems
{
    public class TilesManager : MonoBehaviour
    {
        public List<TileData> tileDataList = new();
        Dictionary<string, TileData> _tileDataDict = new();
        public List<GameObject> obstacleList = new();
        Dictionary<string, GameObject> _obstaclesDict = new();

        public void Initialize()
        {
            InitializeTileData();
            InitializeObstacles();
        }

        void InitializeTileData()
        {
            var data = Resources.LoadAll<TileData>("Data/Tiles");
            tileDataList = new();
            
            foreach (var tile in data)
            {
                _tileDataDict[tile.Id] = tile;
                tileDataList.Add(tile);
            }
        }

        void InitializeObstacles()
        {
            var gos = Resources.LoadAll<GameObject>("Prefabs/Obstacles");
            obstacleList = new();
            
            foreach (var go in gos)
            {
                if (go.TryGetComponent<Tile>(out var tile))
                {
                    _obstaclesDict[tile.TileData.Id] = go;
                    obstacleList.Add(go);
                }
            }
        }

        public TileData GetTileDataFromId(string id)
        {
            if (_tileDataDict.ContainsKey(id))
            {
                return _tileDataDict[id];
            }
            throw null;
        }

        public void PlaceTile(string id, Vector2Int coords)
        {

        }

        public Tile PlaceObstacle(string id, Vector2Int coords)
        {
            if (_obstaclesDict.ContainsKey(id))
            {
                var go = Instantiate(_obstaclesDict[id]);
                var tile = go.GetComponent<Tile>();
                tile.CoordinateToPosition(coords);
                tile.Initialize();
                return tile;
            }
            return null;
        }
    }
}