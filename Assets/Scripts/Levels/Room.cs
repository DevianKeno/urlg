using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using URLG.Enemies;
using URLG.Telemetry;

using static URLG.Generator.Generator.Map;

public enum WeaponType {
    Fireball, Beam, Wave
}

namespace URLG.Levels
{
    public interface IStatistic
    {
        public int Value { get; set; }
        public void Increment();
        public void Decrement();
    }

    public enum WeaponType {
        Fireball, Beam, Wave
    }


    public class TypeofWeapon
    {
        protected WeaponType value;
        public WeaponType Value => value;

        public TypeofWeapon(WeaponType value)
        {
            this.value = value;
        }
    }

    public struct EnemyCount<T> : IStatistic where T : TypeofWeapon
    {
        int value;
        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public void Increment()
        {
            value++;
        }

        public void Decrement()
        {
            value--;
        }
    }

    public struct RoomData
    {
        public Dictionary<Cardinal, RoomData> Neighbors { get; set; }

        public int EnemyCountFire { get; set; }
        public int EnemyCountBeam { get; set; }
        public int EnemyCountWave { get; set; }
        public int ObstacleCountFire { get; set; }
        public int ObstacleCountBeam { get; set; }
        public int ObstacleCountWave { get; set; }
    }

    public class Room : MonoBehaviour, ILoadable
    {
        [Header("Objects")]
        [SerializeField] protected GameObject content;
        public GameObject Content
        {
            get { return content; }
            set { content = value; }
        }

        public Vector2 minBounds;
        public Vector2 maxBounds;
        
        StatCollection _roomStats;
        public StatCollection RoomStats => _roomStats;
        
        [SerializeField] GameObject tilesContainer;
        [SerializeField] GameObject enemiesContainer;
        [SerializeField] GameObject obstaclesLayer;
        [SerializeField] GameObject triggers;

        [SerializeField] List<GameObject> tileLayers = new();
        [SerializeField] List<Tile> tiles = new();

        [Header("Doorways")]
        public bool HasNorthDoor = true;
        public bool HasSouthDoor = true;
        public bool HasEastDoor = true;
        public bool HasWestDoor = true;

        
        [Header("Objects")]
        [SerializeField] RoomDoor northDoor;
        [SerializeField] RoomDoor southDoor;
        [SerializeField] RoomDoor eastDoor;
        [SerializeField] RoomDoor westDoor;


        #region Properties

        public int EnemyCount
        {
            get => 0;
        }
        public int ObstacleCount
        {
            get => 0;
        }
        public int TargetCount => EnemyCount + ObstacleCount;

        #endregion


        #region Initializing methods

        public void Initialize()
        {
            tiles = new List<Tile>();
            tileLayers = new List<GameObject>();
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

            for (int i = 0; i < tilesContainer.transform.childCount; i++)
            {
                var go = tilesContainer.transform.GetChild(i).gameObject;

                if (go.TryGetComponent(out RoomTileLayer tileLayer))
                {
                    ProcessTileLayer(go, i, ref minX, ref maxX, ref minY, ref maxY);
                }
            }

            InitializeStats();
            InitializeDoors();
            CalculateBounds(minX, maxX, minY, maxY);
        }

        void InitializeStats()
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
        }


        public void InitializeTiles()
        {
            foreach (Tile t in tiles)
            {
                t.Initialize();
            }
            northDoor.SetDoorsOpen(HasNorthDoor);
            southDoor.SetDoorsOpen(HasSouthDoor);
            eastDoor.SetDoorsOpen(HasEastDoor);
            westDoor.SetDoorsOpen(HasWestDoor);
        }

        void ProcessTileLayer(GameObject layer, int layerIndex, ref int minX, ref int maxX, ref int minY, ref int maxY)
        {
            layer.name = $"Layer {layerIndex} ({layer.GetComponent<RoomTileLayer>().LayerName})";
            tileLayers.Add(layer);

            foreach (Transform child in layer.transform)
            {
                if (child.TryGetComponent(out Tile tile))
                {
                    InitializeTile(tile, layerIndex, ref minX, ref maxX, ref minY, ref maxY);
                }
            }
        }

        void InitializeTile(Tile tile, int layerIndex, ref int minX, ref int maxX, ref int minY, ref int maxY)
        {
            tile.Initialize();
            // tile.SortingGroup.sortingOrder = layerIndex;
            tiles.Add(tile);

            Vector3 position = tile.transform.position;
            minX = Mathf.Min(minX, (int) position.x);
            maxX = Mathf.Max(maxX, (int) position.x);
            minY = Mathf.Min(minY, (int) position.y);
            maxY = Mathf.Max(maxY, (int) position.y);
        }

        void CalculateBounds(int minX, int maxX, int minY, int maxY)
        {
            minBounds = new(minX, minY);
            maxBounds = new(maxX, maxY);
            
            Debug.Log($"Bounds - Min: {minBounds}, Max: {maxBounds}");
        }

        void InitializeDoors()
        {
            var door = DoorwayType.Door;
            var wall = DoorwayType.Wall;

            northDoor.DoorwayType = HasNorthDoor ? door : wall;
            southDoor.DoorwayType = HasSouthDoor ? door : wall;
            eastDoor.DoorwayType = HasEastDoor ? door : wall;
            westDoor.DoorwayType = HasWestDoor ? door : wall;
        }

        #endregion

        
        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                InitializeDoors();
            }
        }


        #region Public methods

        static Room CreateRoomStart()
        {
            throw new NotImplementedException();
        }

        static Room CreateRoomEnd()
        {
            throw new NotImplementedException();
        }

        static Room CreateRoomStandard()
        {
            throw new NotImplementedException();
        }

        static Room CreateRoomKey()
        {
            throw new NotImplementedException();
        }

        static Room CreateRoomLock()
        {
            throw new NotImplementedException();
        }

        public void ShutDoors()
        {
            StartCoroutine(nameof(ShutDoorsCoroutine));
        }

        public void GenerateFeatures()
        {
            GenerateContent();
        }

        public void GenerateContent()
        {
#if UNITY_EDITOR 
            GenerateObstaclesRandomEditor();
            GenerateEnemiesRandomEditor();
#else
            GenerateObstaclesRandom();
            GenerateEnemiesRandom();
#endif
        }

        public void CountFeatures()
        {
            foreach (Transform c in enemiesContainer.transform)
            {
                if (c.TryGetComponent(out Enemy enemy))
                {
                    if (enemy is FireWeak)
                    {
                        _roomStats["enemyCountFire"].Increment();
                    }
                    else if (enemy is BeamWeak)
                    {
                        _roomStats["enemyCountBeam"].Increment();
                    }
                    else if (enemy is WaveWeak)
                    {
                        _roomStats["enemyCountWave"].Increment();
                    }
                }
            }
            
            foreach (Transform c in obstaclesLayer.transform)
            {
                if (c.TryGetComponent(out Tile tile))
                {
                    if (tile is Glass)
                    {
                        _roomStats["obstacleCountFire"].Increment();
                    }
                    else if (tile is BurnableCrate)
                    {
                        _roomStats["obstacleCountBeam"].Increment();
                    }
                    else if (tile is WaveWeak)
                    {
                        _roomStats["obstacleCountWave"].Increment();
                    }
                }
            }
        }

        public void Load()
        {
            Content.SetActive(true);
        } 

        public void Unload()
        {
            Content.SetActive(false);
        }

        #endregion


        IEnumerator ShutDoorsCoroutine()
        {
            yield return new WaitForSeconds(0.2f);
            
            if (HasNorthDoor)
            {
                northDoor?.SetDoorsOpen(false);
            }
            if (HasSouthDoor)
            {
                southDoor?.SetDoorsOpen(false);
            }
            if (HasEastDoor)
            {
                eastDoor?.SetDoorsOpen(false);
            }
            if (HasWestDoor)
            {
                westDoor?.SetDoorsOpen(false);
            }
        }

        List<Vector2Int> GetRandomCoordinates(int count)
        {
            List<Vector2Int> positions = new();

            for (int i = 0; i < count; i++)
            {
                int x = UnityEngine.Random.Range((int) minBounds.x, (int) maxBounds.x);
                int y = UnityEngine.Random.Range((int) minBounds.y, (int) maxBounds.y);
                positions.Add(new Vector2Int(x, y));
            }
            return positions;
        }
        

        internal IEnumerator OnPlayerEntry()
        {
            ShutDoors();
            GenerateContent();
            // CountFeatures();
            yield return null;
        }

        void GenerateObstaclesRandom()
        {
            /// 0, 7 are limiters, arbitrary values
            var fireObstacleCount = UnityEngine.Random.Range(0, 7);
            var beamObstacleCount = UnityEngine.Random.Range(0, 7);
            var waveObstacleCount = UnityEngine.Random.Range(0, 7);
            
            var rand = GetRandomCoordinates(fireObstacleCount);
            foreach (var coord in rand)
            {
                var obstacle = Game.Tiles.PlaceObstacle("glass_single_bottom", coord);
                obstacle.transform.SetParent(obstaclesLayer.transform);
            }
            
            rand = GetRandomCoordinates(beamObstacleCount);
            foreach (var coord in rand)
            {
                var obstacle = Game.Tiles.PlaceObstacle("crate_single_bottom", coord);
                obstacle.transform.SetParent(obstaclesLayer.transform);
            }
            
            rand = GetRandomCoordinates(waveObstacleCount);
            foreach (var coord in rand)
            {
                
            }
            /// Re-initialize
            Initialize();
        }

        

        void GenerateEnemiesRandom()
        {
            var fireEnemyCount = UnityEngine.Random.Range(0, 10);
            var beamEnemyCount = UnityEngine.Random.Range(0, 10);
            var waveEnemyCount = UnityEngine.Random.Range(0, 10);
        }


        #region Editor
#if UNITY_EDITOR
        public void SetDoorsEditor(bool open)
        {
            northDoor?.SetDoorsOpen(open);
            southDoor?.SetDoorsOpen(open);
            eastDoor?.SetDoorsOpen(open);
            westDoor?.SetDoorsOpen(open);
        }

        public void GenerateObstaclesRandomEditor()
        {
            var fireObstacleCount = UnityEngine.Random.Range(0, 7);
            var beamObstacleCount = UnityEngine.Random.Range(0, 7);
            var waveObstacleCount = UnityEngine.Random.Range(0, 7);

            // PrefabUtility.InstantiatePrefab()
        }

        public void GenerateEnemiesRandomEditor()
        {
            var fireObstacleCount = UnityEngine.Random.Range(0, 7);
            var beamObstacleCount = UnityEngine.Random.Range(0, 7);
            var waveObstacleCount = UnityEngine.Random.Range(0, 7);

            // PrefabUtility.InstantiatePrefab()
        }

        public RoomDoor GetDoor(Cardinal direction)
        {
            return direction switch
            {
                Cardinal.North => northDoor,
                Cardinal.South => southDoor,
                Cardinal.East => eastDoor,
                Cardinal.West => westDoor,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
#endif
        #endregion
    }
}