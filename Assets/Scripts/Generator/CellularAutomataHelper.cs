/*

Program Title: Cellular Automata Helper
Date written: September 21, 2024
Data revised: December 17, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Where the program fits in the general system design:
    This helper is used to create CA patterns for the levels. It also provides visualizations.

Purpose:
    This component generates a noise grid to guide the shape of levels in the procedural map generation system. 
    It includes utilities for creating noise grids, applying cellular automata (CA) smoothing, and generating 
    room shapes with visualizations. Its settings are also configurable via codem but primariy in the Unity inspector.

Control:
    This component is first configured in the Inspector, namely the noise grid and room generation parameters.
    The flow is as follows:
    GenerateNoiseGrid()
        -> CreateNoiseGridRandom(width, height, density)
        -> CreateGridVisualization()
        GenerateRoomShaped()
    The result should be a CA noise pattern alongside a generated room following a shaper respecting the boundaries.

Data Structures:
    List: used to store the generated rooms as an ordered list 
    NoiseGridSettings: used to store settings for generating an instance of a noise grid.
    GenerateRoomShapeCalculations: used to store calculation parameters for shape generation
    GenerateRoomShapeResult: used to store the result of a generated room shape
*/

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using RL.Classifiers;
using RL.Telemetry;
using RL.UI;

namespace RL.CellularAutomata
{
    /// <summary>
    /// Data structure to store settings for generating an instance of a noise grid.
    /// </summary>
    public struct NoiseGridSettings
    {
        public int[,] Grid { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float Density { get; set; }
    }

    /// <summary>
    /// Data structure to store parameters for calculating a room's shape when being generated.
    /// </summary>
    public struct GenerateRoomShapeCalculations
    {
        public Vector2Int MinBounds { get; set; }
        public Vector2Int MaxBounds { get; set; }
        public readonly Vector2Int Size
        {
            get
            {
                return new(
                    System.Math.Abs(MaxBounds.x - MinBounds.x) + 1,
                    System.Math.Abs(MaxBounds.y - MinBounds.y) + 1);
            }
        }
        public Vector3 TotalPosition { get; set; }
    }

    /// <summary>
    /// Data structure to store the result of generated rooms.
    /// </summary>
    public struct GenerateRoomShapeResult
    {
        public List<MockRoom> Rooms { get; set; }
        public GenerateRoomShapeCalculations Calculations { get; set; }
    }

    public class CellularAutomataHelper : MonoBehaviour
    {
        const int DefaultPixelsPerUnit = 120;
        const int FloorTile = 0;
        const int WallTile = 1;
        const int RoomTile = 2;
        const int MaxValue = 100;

        [Header("Noise Grid Settings")]
        public int Height = 20;
        public int Width = 20;
        [Range(0, 100)]
        public int Density = 60;
        public bool VisualizeNoiseGrid = false;

        public bool HasNoiseGrid => noiseGrid != null;
        int[,] noiseGrid;
        List<GameObject> noiseTiles = new();

        [Header("CA Settings")]
        public int NeighborWallThreshold = 4;
        public int Iterations = 3;
        public int[,] currentGrid => gridHistory[_currGridIter];

        List<int[,]> gridHistory = new();
        int _currGridIter = 0; /// current grid iteration

        [SerializeField] GameObject tilePrefab;
        public Color WallTileColor = Color.black;
        public Color FloorTileColor = Color.white;

        [Header("Mock Level Settings")]
        /// <summary>
        /// Count excludes the "Start" and "End" rooms.
        /// </summary>
        public int RoomCount = 1;
        /// <summary>
        /// Whether to included the "Start" room in the generation.
        /// </summary>
        public bool IncludeStartRoom = true;
        /// <summary>
        /// Whether to included the "End" room in the generation.
        /// </summary>
        public bool IncludeEndRoom = true;
        /// <summary>
        /// Override start room coordinates. [Use for testing]
        /// </summary>
        public bool CustomStartRoomCoordinates;
        public Vector2Int StartRoomCoordinates;

        MockRoom _startRoom;
        List<MockRoom> _rooms = new();
        public List<MockRoom> Rooms => _rooms;
        Stack<MockRoom> _previousRooms = new(); /// store "previous rooms" when searching
        /// <summary>
        /// Stores the coordinates of all room that already exists.
        /// </summary>        
        HashSet<Vector2Int> _existingRoomsHashset = new();

        [SerializeField] GameObject mockRoomPrefab;
        [SerializeField] Transform mockRoomContainer;

        public RecolorType RecolorType = RecolorType.BOTH;
        public Color StartRoomColor = Color.green;
        public Color NormalRoomColor = Color.cyan;
        public Color NormalRoomColor2 = new(0.0504183f, 0.8779624f, 0.9716981f); /// lighter cyan for checkerboard pattern
        public Color EndRoomColor = Color.red;

        [Header("UIs")]
        [SerializeField] RDTelemetryUI playerTelemetryUI;
        [SerializeField] GameObject selector;

        
        #region Static methods

        /// <summary>
        /// Generates a simple random noise grid with density parameter.
        /// </summary>
        /// <param name="density">0 - 100</param>
        public static int[,] CreateNoiseGridRandom(int width, int height, float density)
        {
            int[,] grid = new int[width, height];
            density = Mathf.Clamp(density, 0f, 100f);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = UnityEngine.Random.Range(0, 101) < density ? WallTile : FloorTile;
            }

            return grid;
        }

        #endregion


        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                foreach (MockRoom room in _rooms)
                {
                    room.Recolor(RecolorType);
                }
            }
        }

// #if UNITY_EDITOR
        #region Public methods

        public void GenerateNoiseGrid()
        {
            noiseGrid = CreateNoiseGridRandom(Width, Height, Density);
            gridHistory.Clear();
            gridHistory.Add(noiseGrid);
            _currGridIter = 0;

            if (VisualizeNoiseGrid)
            {
                CreateGridVisualization(noiseGrid);
            }

            Debug.Log($"Created noise grid of size ({Width} x {Height})");
        }

        /// <summary>
        /// Increment grid iteration for CA smoothing.
        /// </summary>
        public void IncrementIteration()
        {
            if (_currGridIter < Iterations)
            {
                var latestGrid = gridHistory[_currGridIter];
                var nextGrid = GenerateShapeCA(latestGrid);

                gridHistory.Add(nextGrid);
                _currGridIter++;
                GenerateCellGrid(nextGrid);
                
                Debug.Log($"Incremented noise grid, current value: {_currGridIter}");
            }
        }

        /// <summary>
        /// Decrement grid iteration for CA roughing.
        /// </summary>
        public void DecrementIteration()
        {
            if (_currGridIter > 0)
            {
                _currGridIter--;
                var previousGrid = gridHistory[_currGridIter];
                GenerateCellGrid(previousGrid);

                Debug.Log($"Decremented noise grid, current value: {_currGridIter}");
            }
        }

        public void GenerateRD()
        {
            GenerateRoomShaped(playerTelemetryUI.GetEntry(StatKey.RoomCount).Value);
        }

        /// <summary>
        /// Generates a random shaped level with specified room count.
        /// Room count does not include start and end rooms.
        /// </summary>
        /// <param name="featurize">Whether to generate room features.</param>
        /// <returns></returns>
        public GenerateRoomShapeResult GenerateRoomShaped(int roomCount, bool featurize = true, bool instantiateObjects = false)
        {
            ClearRooms();
            if (!HasNoiseGrid)
            {
                GenerateNoiseGrid();
            }
            RefreshGridAll();

            if (instantiateObjects)
            {
                if (mockRoomContainer != null)
                {
                    DestroyAllRooms();
                }
                mockRoomContainer = new GameObject("Rooms").transform;
                mockRoomContainer.SetParent(transform);
            }

            var result = new GenerateRoomShapeResult
            {
                Rooms = _rooms
            };
            _rooms = new List<MockRoom>();
            Vector2Int coords;

            if (CustomStartRoomCoordinates)
            {
                if (currentGrid[StartRoomCoordinates.x, StartRoomCoordinates.y] == WallTile)
                {
                    Debug.LogError("Starting room cannot be a wall tile");
                    return result;
                }
                coords = StartRoomCoordinates;
            }
            else
            {
                do
                {
                    coords = new Vector2Int(UnityEngine.Random.Range(0, Width), UnityEngine.Random.Range(0, Height));
                }
                while (currentGrid[coords.x, coords.y] == WallTile);
            }

            /// Create the starting room
            MockRoom newRoom = InstantiateRoom(coords.x, coords.y, StartRoomColor);
            currentGrid[coords.x, coords.y] = RoomTile;
            _existingRoomsHashset.Add(coords);
            _rooms.Add(newRoom);
            _startRoom = newRoom;
            _startRoom.IsStartRoom = true;
            FeaturizeEmpty(_startRoom);
            // SubscribeRoomEvents(newRoom);
            
            Vector2Int minBounds = new(_startRoom.x, _startRoom.y);
            Vector2Int maxBounds = new(_startRoom.x, _startRoom.y);
            Vector3 totalPosition = Vector3.zero;

            AddCalculations(newRoom);

            var roomsLeft = roomCount; /// Number of rooms left to generate
            _previousRooms.Push(_startRoom);

            MockRoom CreateNewRoom(Color color)
            {
                MockRoom currentRoom = _previousRooms.Peek(); /// Look at the current room on top of the stack
                if (currentRoom == null) return null;

                /// Try to create a room at a neighboring tile
                if (CreateRoomAtNeighbor(currentRoom, color, out var createdRoom))
                {
                    roomsLeft--; /// Successfully created a room, decrement the count
                }
                else /// If no room could be created, backtrack by popping the current room off the stack
                {
                    _previousRooms.Pop();
                }

                AddCalculations(createdRoom);
                return createdRoom;
            }

            /// <summary>
            /// Necessary calculations
            /// </summary>
            void AddCalculations(MockRoom room)
            {
                if (room == null) return;
                
                totalPosition += room.transform.position;
                minBounds.x = System.Math.Min(minBounds.x, room.x);
                minBounds.y = System.Math.Min(minBounds.y, room.y);
                maxBounds.x = System.Math.Max(maxBounds.x, room.x);
                maxBounds.y = System.Math.Max(maxBounds.y, room.y);
            }

            /// Generate rooms until all rooms are placed
            while (roomsLeft > 0)
            {
                if (_previousRooms.Any())
                {
                    /// applies a checkered-colored pattern to the generated rooms for aesthetic purposes
                    // var checkeredColor = roomsLeft % 2 == 0 ? NormalRoomColor : NormalRoomColor2;
                    var newInterRoom = CreateNewRoom(Color.white);
                    if (newInterRoom == null) continue;
                    
                    if (featurize) FeaturizeRandom(newInterRoom);
                    newInterRoom.Recolor(RecolorType);
                    // SubscribeRoomEvents(newInterRoom);
                }
                else
                {
                    Debug.LogError("Backtracking failed, no more rooms to backtrack to.");
                    break;
                }
            }
            
            if (IncludeEndRoom && _previousRooms.Any()) ///HMMMMMMMMMMMMMMM
            {
                CreateRoomAtNeighbor(_previousRooms.Peek(), EndRoomColor, out var endRoom);
                endRoom.IsEndRoom = true;
                FeaturizeEmpty(endRoom);
                // SubscribeRoomEvents(endRoom);
                AddCalculations(endRoom);
            }

            if (roomsLeft > 0)
                Debug.LogError("Failed to generate all rooms");
            else
                Debug.Log("Generated all rooms successfully");
            
            result.Rooms = _rooms;
            result.Calculations = new()
            {
                MinBounds = minBounds,
                MaxBounds = maxBounds,
                TotalPosition = totalPosition,
            };

            return result;
        }

        public void RefreshGridAll()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (currentGrid[x, y] == RoomTile) currentGrid[x, y] = FloorTile;
            }
            _existingRoomsHashset.Clear();
        }

        public void DestroyAllRooms()
        {
            if (mockRoomContainer == null) return;

            #if UNITY_EDITOR
                DestroyImmediate(mockRoomContainer.gameObject);
            #else
                Destroy(mockRoomContainer.gameObject);
            #endif
        }
        
        #endregion

    
        #region Private methods

        /// <summary>
        /// Adds generated features to target room.
        /// </summary>
        void Featurize(MockRoom room, FeatureParametersSettings settings)
        {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var generatedFeatureParameters = GaussianNaiveBayes.GenerateFeatureOptionsRandom(settings, seed);
            room.GenerateFeatures(generatedFeatureParameters);
        }
        
        /// <summary>
        /// Adds randomly generated features to the target room.
        /// </summary>
        void FeaturizeRandom(MockRoom room)
        {
            if (room == null) return;
            
            var settings = new FeatureParametersSettings()
            {
                MaxEnemyCount = 15,
                MaxObstacleCount = 6,
            };
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var generatedFeatureParameters = GaussianNaiveBayes.GenerateFeatureOptionsRandom(settings, seed);
            room.GenerateFeatures(generatedFeatureParameters);
        }

        /// <summary>
        /// Adds empty features to the target room.
        /// </summary>
        void FeaturizeEmpty(MockRoom room)
        {
            var settings = new FeatureParametersSettings()
            {
                MaxEnemyCount = 0,
                MaxObstacleCount = 0,
            };
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var generatedFeatureParameters = GaussianNaiveBayes.GenerateFeatureOptionsRandom(settings, seed);
            room.GenerateFeatures(generatedFeatureParameters);
        }

        /// <summary>
        /// Generates a random neightboring room from the target rooom.
        /// </summary>
        /// <param name="currentRoom">Target room</param>
        /// <param name="roomColor">Defined color for visualization</param>
        /// <param name="result">The new neighboring room</param>
        /// <returns>Whether a room is generated successfully</returns>
        bool CreateRoomAtNeighbor(MockRoom currentRoom, Color roomColor, out MockRoom result)
        {
            var neighbors = GetFloorNeighbors(currentRoom.x, currentRoom.y);
            if (neighbors.Length <= 0)
            {
                result = null;
                return false; /// no neighbors, fail
            }

            var pickedNeighbor = PickRandomNeighbor(neighbors);

            /// Place new room at picked neighbor position
            MockRoom newRoom;
            newRoom = InstantiateRoom(pickedNeighbor.x, pickedNeighbor.y, roomColor);

            /// Map passage ways between neighbors
            /// Currently just a straight path to the exit
            if (_previousRooms.Any())
            {
                var previousRoom = _previousRooms.Peek();
                
                newRoom.ConnectDoorways(previousRoom);
                newRoom.ToggleDoorway(newRoom.DirectionTo(previousRoom), isOpen: true);
            }

            _rooms.Add(newRoom);
            _previousRooms.Push(newRoom);

            result = newRoom;
            return true; /// successfully placed new room
        }

        /// <summary>
        /// Helper function to pick a random neighbor.
        /// </summary>
        public static Vector2Int PickRandomNeighbor(Vector2Int[] neighbors)
        {
            return neighbors[UnityEngine.Random.Range(0, neighbors.Length)];
        }

        /// <summary>
        /// Validate cell if within bounds of the given grid.
        /// </summary>
        public bool WithinBounds(int[,] grid, int x, int y)
        {
            return (x >= 0 && x < Width) && (y >= 0 && y < Height);
        }

        /// <summary>
        /// Get all valid floor neighbors of target cell coordinate.
        /// </summary>
        Vector2Int[] GetFloorNeighbors(int x, int y)
        {
            var neighbors = new Vector2Int[]
            {
                new(x, y + 1),
                new(x, y - 1),
                new(x + 1, y),
                new(x - 1, y),
            };

            neighbors = neighbors
                .Where(n => WithinBounds(currentGrid, n.x, n.y) /// select valid tiles
                    && currentGrid[n.x, n.y] == FloorTile)      /// select all floor tiles
                .ToArray();
            return neighbors;
        }

        /// <summary>
        /// Helper function to instantiate a room at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <returns>The instantiated room</returns>
        MockRoom InstantiateRoom(int x, int y, Color color)
        {
#if UNITY_EDITOR
            var room = (GameObject) PrefabUtility.InstantiatePrefab(mockRoomPrefab, mockRoomContainer);
#else
            var room = Instantiate(mockRoomPrefab, mockRoomContainer);
#endif
            var spriteRenderer = room.GetComponent<SpriteRenderer>();
            
            spriteRenderer.color = color;
            room.transform.SetPositionAndRotation(
                new Vector3(x, y, 0f),
                Quaternion.identity
            );

            var mockRoom = room.GetComponent<MockRoom>();
            mockRoom.Coordinates = new(x, y);
            currentGrid[x, y] = RoomTile;
            return mockRoom;
        }

        void CreateGridVisualization(int[,] grid)
        {
            ClearGeneratedTiles();

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
#if UNITY_EDITOR
            var tile = (GameObject) PrefabUtility.InstantiatePrefab(tilePrefab, transform);
#else
            var tile = Instantiate(tilePrefab, transform);
#endif
                var spriteRenderer = tile.GetComponent<SpriteRenderer>();

                if (noiseGrid[x, y] == WallTile)
                {
                    spriteRenderer.color = WallTileColor;
                }
                else if (noiseGrid[x, y] == FloorTile)
                {
                    spriteRenderer.color = FloorTileColor;
                }

                tile.transform.SetPositionAndRotation(
                    new Vector3(x, y, 0f),
                    Quaternion.identity
                );

                noiseTiles.Add(tile);
            }
        }

        void GenerateCellGrid(int[,] grid)
        {
            ClearGeneratedTiles();

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
#if UNITY_EDITOR
                var tile = (GameObject) PrefabUtility.InstantiatePrefab(tilePrefab, transform);
#else
                var tile = Instantiate(tilePrefab, transform);
#endif
                var spriteRenderer = tile.GetComponent<SpriteRenderer>();

                if (grid[x, y] == WallTile)
                {
                    spriteRenderer.color = Color.black;
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }

                tile.transform.SetPositionAndRotation(
                    new Vector3(x, y, 0f),
                    Quaternion.identity
                );

                noiseTiles.Add(tile);
            }
        }

        void ClearGeneratedTiles()
        {
            foreach (var go in noiseTiles)
            {
                DestroyImmediate(go);
            }
            noiseTiles.Clear();
        }

        void ClearRooms()
        {
            foreach (var room in _rooms)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            _rooms.Clear();
            _previousRooms.Clear();
        }

        void RefreshGrid()
        {
            foreach (var coord in _existingRoomsHashset)
            {
                currentGrid[coord.x, coord.y] = FloorTile;
            }
            _existingRoomsHashset.Clear();
        }

        void SubscribeRoomEvents(MockRoom room)
        {
            if (SceneManager.GetActiveScene().name != "R&D") return;

            room.OnClick += playerTelemetryUI.OnRoomClick;
            room.OnClick += SelectorToRoom;
        }

        void SelectorToRoom(MockRoom room)
        {
            if (selector == null)
            {
                selector = Instantiate(Resources.Load<GameObject>("Prefabs/RD/Selector"), transform);
            }
            LeanTween.cancel(selector);
            LeanTween.scale(selector, Vector3.zero, 0f);
            LeanTween.scale(selector, new(1.05f, 1.05f, 1f), 0.05f).setEaseOutSine();
            selector.transform.position = new(room.x, room.y, -1);
        }

        /// generate shapes using ca rules and NeighborWallThreshold
        int[,] GenerateShapeCA(int[,] noiseGrid)
        {
            int[,] grid = new int[Width, Height];

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                var nbWalls = GetWallNeighborsCount(noiseGrid, x, y);
                grid[x, y] = nbWalls >= NeighborWallThreshold ? WallTile : FloorTile;
            }

            return grid;
        }

        int GetWallNeighborsCount(int[,] grid, int x, int y)
        {
            int neighboringWallsCount = 0;

            for (int i = x - 1; i <= x + 1; i++)
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i < Width && j >= 0 && j < Height)
                {
                    if (i != x || j != y)
                    {
                        neighboringWallsCount += (grid[i, j] == WallTile) ? 1 : 0;
                    }
                }
                else
                {
                    neighboringWallsCount++;
                }
            }

            return neighboringWallsCount;
        }
        #endregion
        // #endif
    }
}
