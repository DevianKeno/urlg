using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RL.Entities;
using RL.Telemetry;

using static RL.Generator.Generator.Map;
using RL.CellularAutomata;
using RL.UI;
using Unity.Collections;

namespace RL.Levels
{
    public class Room : MonoBehaviour, ILoadable
    {
        FeatureParameters features;
        public FeatureParameters Features => features;
        RoomStatCollection roomStats;
        public RoomStatCollection Stats => roomStats;

        public bool IsActive;
        public bool IsCleared;
        public bool IsStartRoom;
        public bool IsEndRoom;
        public bool HasNextRoom;
        public Room PreviousRoom;
        [SerializeField] Room nextRoom;
        public Room NextRoom
        {
            get { return nextRoom; }
            set
            {
                HasNextRoom = value != null;
                nextRoom = value;
            }
        }
        
        [SerializeField] Vector2 minBounds;
        public Vector2 MinBounds => minBounds;
        [SerializeField] Vector2 maxBounds;
        public Vector2 MaxBounds => maxBounds;
        [SerializeField] Vector2Int size;
        public Vector2Int Size => size;
        
        List<Entity> enemies = new();
        [SerializeField] int remainingEnemies;
        [SerializeField] int remainingObstacles;
        
        [SerializeField] List<GameObject> tileLayers = new();
        [SerializeField] List<Tile> tiles = new();
        [SerializeField] Dictionary<Vector2Int, Tile> tileCoords = new();
        /// <summary>
        /// Tile coordinates already populated with an obstacle or enemy.
        /// </summary>
        List<Vector2Int> populatedTileCoordinates = new();

        [Header("Objects")]
        [SerializeField] protected GameObject content;
        public GameObject Content
        {
            get { return content; }
            set { content = value; }
        }
        public Transform Center;
        [SerializeField] GameObject tilesContainer;
        [SerializeField] GameObject enemiesContainer;
        [SerializeField] GameObject obstaclesLayer;
        [SerializeField] GameObject triggers;

        [Header("Doorways")]
        public bool HasNorthDoor = true;
        [SerializeField] RoomDoor northDoor;
        public bool HasSouthDoor = true;
        [SerializeField] RoomDoor southDoor;
        public bool HasEastDoor = true;
        [SerializeField] RoomDoor eastDoor;
        public bool HasWestDoor = true;
        [SerializeField] RoomDoor westDoor;


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
            CalculateSize();
        }

        void InitializeStats()
        {
            roomStats = new(Telemetry.Telemetry.RoomStatsKeys);
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
            
            tiles.Add(tile);
            tileCoords[tile.Coordinates] = tile;

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

        void CalculateSize()
        {
            size = new Vector2Int((int) (maxBounds.x - minBounds.x + 1), (int)  (maxBounds.y - minBounds.y + 1));
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

        public void Featurize(RoomStatCollection roomStats)
        {
            this.roomStats = roomStats;
            this.remainingEnemies = roomStats.TotalEnemyCount;
            this.remainingObstacles = roomStats.TotalObstacleCount;
            
            // GenerateObstaclesCorners();
            GenerateObstacles(
                roomStats.GetStat(StatKey.ObstacleCountFire).Value,
                roomStats.GetStat(StatKey.ObstacleCountBeam).Value,
                roomStats.GetStat(StatKey.ObstacleCountWave).Value);

            // GenerateEnemiesCorners();
            GenerateEnemies(
                roomStats.GetStat(StatKey.EnemyCountFire).Value,
                roomStats.GetStat(StatKey.EnemyCountBeam).Value,
                roomStats.GetStat(StatKey.EnemyCountWave).Value
            );

            Initialize(); /// Re-initialize
        }

        public Tile GetTile()
        {
            return null;
        }

        public void OpenDoors()
        {
            if (HasNorthDoor) northDoor?.Open();
            if (HasSouthDoor) southDoor?.Open();
            if (HasEastDoor) eastDoor?.Open();
            if (HasWestDoor) westDoor?.Open();
        }

        public void ShutDoors()
        {
            StartCoroutine(nameof(ShutDoorsCoroutine));
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


        IEnumerator FinishRoom()
        {
            IsActive = false;
            IsCleared = true;
            Game.UI.ShowArrowPointer();
            OpenDoors();
            yield return new WaitForSeconds(0.5f);

            Game.Main.Player.SetControlsEnabled(false);
            Game.Telemetry.SaveCurrentRoomStats();

            var go = Resources.Load<GameObject>("Prefabs/UI/Likert Scale UI");
            var likertUi = Instantiate(go, Game.UI.transform).GetComponent<LikertScaleUI>();
            likertUi.SetTargetRoom(this);
            likertUi.OnClose += () =>
            {
                var go = Resources.Load<GameObject>("Prefabs/UI/SwapWeaponsWindow");
                var sww = Instantiate(go, Game.UI.transform).GetComponent<SwapWeaponsWindow>();

                sww.OnClose += () =>
                {
                    Game.Main.Player.SetControlsEnabled(true);
                };
            };
        }

        IEnumerator ShutDoorsCoroutine()
        {
            yield return new WaitForSeconds(0.2f);

            Game.Audio.Play("door_close");
            
            if (HasNorthDoor) northDoor?.Close();
            if (HasSouthDoor) southDoor?.Close();
            if (HasEastDoor) eastDoor?.Close();
            if (HasWestDoor) westDoor?.Close();
        }

        const int MaxIter = 255;
        List<Vector2Int> GetRandomCoordinatesObstacle(int count)
        {
            List<Vector2Int> positions = new();
            int attempts = 0;
            int successCount = 0;

            while (attempts < MaxIter && successCount < count)
            {
                Vector2Int coord = new(
                    UnityEngine.Random.Range(3, size.x - 3), 
                    UnityEngine.Random.Range(4, size.y - 4));

                if (populatedTileCoordinates.Contains(coord))
                {
                    attempts++;
                    continue;
                }
                else
                {
                    positions.Add(coord);
                    populatedTileCoordinates.Add(coord);
                    successCount++;
                    attempts++;
                }
            }

            return positions;
        }
        List<Vector2Int> GetRandomCoordinatesEnemy(int count)
        {
            List<Vector2Int> positions = new();
            int attempts = 0;
            int successCount = 0;

            while (attempts < MaxIter && successCount < count)
            {
                Vector2Int coord = new(
                    UnityEngine.Random.Range(3, size.x - 5), 
                    UnityEngine.Random.Range(4, size.y - 7));

                if (populatedTileCoordinates.Contains(coord))
                {
                    attempts++;
                    continue;
                }
                else
                {
                    positions.Add(coord);
                    populatedTileCoordinates.Add(coord);
                    successCount++;
                    attempts++;
                }
            }

            return positions;
        }
        
        void RegisterEnemy(Enemy enemy)
        {
            enemy.OnDeath += OnEnemyKilled;
        }

        void OnEnemyKilled(Enemy enemy)
        {
            if (enemy == null) return; 
            
            if (enemies.Contains(enemy))
            {
                enemies.Remove(enemy);
            }
            remainingEnemies--;

            if (enemies.Count <= 0 || remainingEnemies <= 0)
            {
                StartCoroutine(FinishRoom());
            }
        }
        
        internal IEnumerator OnPlayerEntry()
        {
            if (IsStartRoom || IsEndRoom || IsCleared) yield break;

            Game.Main.CurrentRoom = this;
            Game.Telemetry.NewRoomStatInstance();
            Game.UI.HideArrowPointer();
            IsActive = true;
            ShutDoors();
            yield return null;
        }

        void GenerateObstaclesCorners()
        {
            var rand = new Vector2Int[]
            {
                new (0, 0),
                new (Size.x, 0),
                new (0, Size.y - 4),
                new (Size.x, Size.y - 4),
            };
            foreach (var coord in rand)
            {
                Game.Tiles.PlaceObstacle("glass", coord, onPlace: (tile) =>
                {
                    tile.transform.SetParent(obstaclesLayer.transform);
                    tile.CoordinateToLocalPosition(coord);
                });
            }
        }

        void GenerateObstacles(int countFire, int countBeam, int countWave)
        {
            var rand = GetRandomCoordinatesEnemy(countFire);
            foreach (var coord in rand)
            {
                string id;
                if (UnityEngine.Random.Range(0, 100) <= 66)
                    id = "crate";
                else
                    id = "crate_double";
                
                Game.Tiles.PlaceObstacle(id, coord, onPlace: (tile) =>
                {
                    tile.transform.SetParent(obstaclesLayer.transform);
                    tile.CoordinateToLocalPosition(coord);
                });
            }
            
            rand = GetRandomCoordinatesEnemy(countBeam);
            foreach (var coord in rand)
            {

            }
            
            rand = GetRandomCoordinatesEnemy(countWave);
            foreach (var coord in rand)
            {
                Game.Tiles.PlaceObstacle("glass", coord, onPlace: (tile) =>
                {
                    tile.transform.SetParent(obstaclesLayer.transform);
                    tile.CoordinateToLocalPosition(coord);
                });
            }
        }
        
        void GenerateEnemiesCorners()
        {
            var rand = new Vector2Int[]
            {
                new (0, 0),
                new (Size.x - 5, 0),
                new (0, Size.y - 7),
                new (Size.x - 5, Size.y - 7),
            };
            foreach (var coord in rand)
            {
                Game.Entity.Spawn("salaman", onSpawn: (entity) =>
                {
                    entity.transform.SetParent(enemiesContainer.transform);
                    entity.LocalPosition = new Vector3(coord.x, coord.y);
                });
            }
        }

        void GenerateEnemies(int countFire, int countBeam, int countWave)
        {
            var rand = GetRandomCoordinatesEnemy(countFire);
            foreach (var coord in rand)
            {
                Game.Entity.Spawn("salaman", onSpawn: (salaman) =>
                {
                    salaman.transform.SetParent(enemiesContainer.transform);
                    salaman.LocalPosition = new Vector3(coord.x, coord.y);
                    enemies.Add(salaman);
                });
                // Game.Entity.Spawn("deer", onSpawn: (entity) =>
                // {
                //     entity.transform.SetParent(enemiesContainer.transform);
                //     entity.LocalPosition = new Vector3(coord.x, coord.y);
                // });
            }
            
            rand = GetRandomCoordinatesEnemy(countBeam);
            foreach (var coord in rand)
            {
                Game.Entity.Spawn("salaman", onSpawn: (salaman) =>
                {
                    salaman.transform.SetParent(enemiesContainer.transform);
                    salaman.LocalPosition = new Vector3(coord.x, coord.y);
                    enemies.Add(salaman);
                });
                // Game.Entity.Spawn("armadillo", onSpawn: (entity) =>
                // {
                //     entity.transform.SetParent(enemiesContainer.transform);
                //     entity.LocalPosition = new Vector3(coord.x, coord.y);
                // });
            }
            
            rand = GetRandomCoordinatesEnemy(countWave);
            foreach (var coord in rand)
            {
                Game.Entity.Spawn("salaman", onSpawn: (salaman) =>
                {
                    salaman.transform.SetParent(enemiesContainer.transform);
                    salaman.LocalPosition = new Vector3(coord.x, coord.y);
                    enemies.Add(salaman);
                });
            }
            
            remainingEnemies = countFire + countBeam + countWave;
        }


        #region Editor

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

        internal RoomDoor SetDoorwayAs(DoorwayType type, Cardinal direction, bool opened)
        {
            switch (direction)
            {
                case Cardinal.North:
                {
                    HasNorthDoor = true;
                    northDoor.DoorwayType = type;
                    if (opened) northDoor.Open();
                    return northDoor;
                }
                case Cardinal.South:
                {
                    HasSouthDoor = true;
                    southDoor.DoorwayType = type;
                    if (opened) southDoor.Open();
                    return southDoor;
                }
                case Cardinal.East:
                {
                    HasEastDoor = true;
                    eastDoor.DoorwayType = type;
                    if (opened) eastDoor.Open();
                    return eastDoor;
                }
                case Cardinal.West:
                {
                    HasWestDoor = true;
                    westDoor.DoorwayType = type;
                    if (opened) westDoor.Open();
                    return westDoor;
                }
                default:
                {
                    return null;
                }
            }
        }

        #endregion
    }
}