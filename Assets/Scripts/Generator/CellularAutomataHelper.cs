using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;
using Cinemachine;

using static URLG.Generator.Generator.Map;

namespace URLG.CellularAutomata
{
    public struct NoiseGridSettings
    {
        public int[,] Grid { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float Density { get; set; }
    }

    public class CellularAutomataHelper : MonoBehaviour
    {
        const int DefaultPixelsPerUnit = 120;
        const int FloorTile = 0;
        const int WallTile = 1;
        const int RoomTile = 2;
        const int MaxValue = 100;

        static Color WallTileColor = Color.black;
        static Color FloorTileColor = Color.white;
        static Color StartRoomColor = Color.green;
        static Color NormalRoomColor = Color.cyan;
        static Color NormalRoomColor2 = new(0.0504183f, 0.8779624f, 0.9716981f);
        static Color EndRoomColor = Color.red;

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

        [Header("Mock Level Settings")]
        /// <summary>
        /// Excluding the Start and End rooms.
        /// </summary>
        public int RoomCount = 1;
        public bool IncludeStartRoom = true; /// unhandled
        public bool IncludeEndRoom = true;
        public bool CustomStartRoomCoordinates;
        public Vector2Int StartRoomCoordinates;
        public float Scaling = 1.0f;

        MockRoom _startRoom;
        List<MockRoom> _rooms = new();
        public List<MockRoom> Rooms => _rooms;
        Stack<MockRoom> _previousRooms = new(); /// "previous rooms when searching"
        HashSet<Vector2Int> _existingRoomsHashset = new();

        [SerializeField] GameObject mockRoomPrefab;
        [SerializeField] Transform mockRoomContainer;
        [SerializeField] CinemachineVirtualCamera virtualCamera;


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
                
                Debug.Log($"Incremented noise grid");
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

                Debug.Log($"Decremented noise grid");
            }
        }

        public void GenerateRD()
        {
            GenerateRooms(RoomCount); /// replace with value from input field
        }

        public List<MockRoom> GenerateRooms(int roomCount)
        {
            ClearRooms();
            if (!HasNoiseGrid)
            {
                GenerateNoiseGrid();
            }
            RefreshGridAll();

            if (mockRoomContainer != null)
            {
                DestroyAllRooms();
            }
            mockRoomContainer = new GameObject("Rooms").transform;
            mockRoomContainer.SetParent(transform);

            _rooms = new List<MockRoom>();
            Vector2Int coords;

            if (CustomStartRoomCoordinates)
            {
                if (currentGrid[StartRoomCoordinates.x, StartRoomCoordinates.y] == WallTile)
                {
                    Debug.LogError("Starting room cannot be a wall tile");
                    return _rooms;
                }
                coords = StartRoomCoordinates;
            }
            else
            {
                do
                {
                    coords = new Vector2Int(
                        UnityEngine.Random.Range(0, Width),
                        UnityEngine.Random.Range(0, Height));
                }
                while (currentGrid[coords.x, coords.y] == WallTile);
            }

            /// Create the starting room
            MockRoom newRoom = CreateRoom(coords.x, coords.y, StartRoomColor);
            currentGrid[coords.x, coords.y] = RoomTile;
            _existingRoomsHashset.Add(coords);
            _rooms.Add(newRoom);
            _startRoom = newRoom;
            
            Vector2Int minBounds = new(_startRoom.x, _startRoom.y);
            Vector2Int maxBounds = new(_startRoom.x, _startRoom.y);
            Vector3 totalPosition = Vector3.zero;
            AddCalculations(newRoom);

            var roomsLeft = roomCount; /// Number of rooms left to generate
            _previousRooms.Push(_startRoom);

            void CreateNewRoom(Color color)
            {
                MockRoom currentRoom = _previousRooms.Peek(); /// Look at the current room on top of the stack

                /// Try to create a room at a neighboring tile
                if (CreateRoomAtNeighbor(currentRoom, color, out _))
                {
                    roomsLeft--; /// Successfully created a room, decrement the count
                }
                else
                {
                    /// If no room could be created, backtrack by popping the current room off the stack
                    _previousRooms.Pop();
                }

                AddCalculations(currentRoom);
            }

            /// Necessary calculations
            void AddCalculations(MockRoom room)
            {
                if (room == null) return;
                
                totalPosition += room.transform.position;
                minBounds.x = Math.Min(minBounds.x, room.x);
                minBounds.y = Math.Min(minBounds.y, room.y);
                maxBounds.x = Math.Max(maxBounds.x, room.x);
                maxBounds.y = Math.Max(maxBounds.y, room.y);
            }

            /// Generate rooms until all rooms are placed
            while (roomsLeft > 0)
            {
                if (_previousRooms.Any())
                {
                    var checkeredColor = roomsLeft % 2 == 0 ? NormalRoomColor : NormalRoomColor2;
                    CreateNewRoom(checkeredColor);
                }
                else
                {
                    Debug.LogError("Backtracking failed, no more rooms to backtrack to.");
                    break;
                }
            }
            
            if (IncludeEndRoom)
            {
                if (_previousRooms.Any()) ///HMMMMMMMMMMMMMMM
                {
                    CreateRoomAtNeighbor(_previousRooms.Peek(), EndRoomColor, out var result);
                    AddCalculations(result);
                }
            }

            if (roomsLeft > 0)
            {
                Debug.LogError("Failed to generate all rooms");
            }
            else
            {
                Debug.Log("Generated all rooms successfully");
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.name == "R&D") /// if on research mode
            {
                Vector2Int size = new(
                    Math.Abs(maxBounds.x - minBounds.x) + 1,
                    Math.Abs(maxBounds.y - minBounds.y) + 1
                );
                var squ = size.x * size.y;
                print($"Mock Level size: ({size.x} x {size.y}), {squ} squ.");
                var ppc = Camera.main.GetComponent<PixelPerfectCamera>();
                ppc.assetsPPU = (int) (DefaultPixelsPerUnit / squ * 100 * Scaling);
                
                /// position camera on cells centroid
                Vector3 centroid = totalPosition / mockRoomContainer.childCount;
                virtualCamera.transform.position = new(
                    centroid.x,
                    centroid.y,
                    -10f);
            }

            return _rooms;
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
        ///
        /// </summary>
        /// <returns>Whether a room is generated successfully.</returns>
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
            newRoom = CreateRoom(pickedNeighbor.x, pickedNeighbor.y, roomColor);

            /// Map passage ways between neighbors
            /// Currently just a straight path to the exit
            if (_previousRooms.Any())
            {
                var previousRoom = _previousRooms.Peek();
                
                newRoom.Connect(previousRoom);
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

        MockRoom CreateRoom(int x, int y, Color color)
        {
            var room = (GameObject) PrefabUtility.InstantiatePrefab(mockRoomPrefab, mockRoomContainer);
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
                var tile = (GameObject) PrefabUtility.InstantiatePrefab(tilePrefab, transform);
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
                var tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, transform);
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
