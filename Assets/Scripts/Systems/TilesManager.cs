/*

Component Title: Tiles Manager
Data written: June 22, 2024
Date revised: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    

Control:


Data Structures:
    Dictionary: used to store all tile and obstacle data
        Key is the Id of the tile/obstacle; Value is the tile/obstacle data itself
    List: used to store the tile and obstacle data(s) as a read-only list
*/

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using RL.Levels;

namespace RL.Systems
{
    public class TilesManager : MonoBehaviour
    {
        public List<TileData> tileDataList = new();
        Dictionary<string, TileData> _tileDataDict = new();
        public List<ObstacleData> obstacleList = new();
        Dictionary<string, ObstacleData> _obstaclesDict = new();

        public void Initialize()
        {
            InitializeTileData();
            InitializeObstacles();
        }

        void InitializeTileData()
        {
            tileDataList = new();
            
            foreach (var data in Resources.LoadAll<TileData>("Data/Tiles"))
            {
                _tileDataDict[data.Id] = data;
                tileDataList.Add(data);
            }
        }

        void InitializeObstacles()
        {
            obstacleList = new();
            
            foreach (var data in Resources.LoadAll<ObstacleData>("Data/Obstacles"))
            {
                _obstaclesDict[data.Id] = data;
                obstacleList.Add(data);
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

        public void PlaceObstacle(string id, Vector2Int coords, Action<Tile> onPlace = null)
        {
            if (_obstaclesDict.ContainsKey(id))
            {
                var op = Addressables.LoadAssetAsync<GameObject>(_obstaclesDict[id].AssetReference);
                op.Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result);
                        if (go.TryGetComponent(out Tile tile))
                        {
                            tile.CoordinateToPosition(coords);
                            tile.Initialize();
                            onPlace?.Invoke(tile);
                            return;
                        }
                        Destroy(go);
                    }
                };
            }
        }
    }
}