using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RL.Classifiers;
using RL.Player;
using RL.Generator;

namespace RL.Levels
{
    public struct RoomMaxParameters
    {
        public int MaxEnemyCount { get; set; }
        public int MaxObstacleCount { get; set; }

        public RoomMaxParameters(int maxEnemyCount, int maxObstacleCount)
        {
            this.MaxEnemyCount = maxEnemyCount;
            this.MaxObstacleCount = maxObstacleCount;
        } 
    }

    public class Level : MonoBehaviour
    {
        /// <summary>
        /// Key is level number, Value is # of rooms on that level.
        /// </summary>
        public static Dictionary<int, int> RoomsPerLevel = new()
        {
            {1, 2}, {2, 2}, {3, 3}, {4, 3}, {5, 4}, {6, 4}, {7, 5}, {8, 5}, {9, 6}, {10, 6}, 
        };
        /// <summary>
        /// Key is level number, Value is RoomMaxParameters.
        /// </summary>
        public static Dictionary<int, RoomMaxParameters> MaxPerLevel = new()
        {
            {1, new(1, 3)},
            {2, new(2, 4)},
            {3, new(2, 4)},
            {4, new(3, 5)},
            {5, new(3, 6)},
            {6, new(3, 6)}, 
            {7, new(4, 8)}, 
            {8, new(4, 8)}, 
            {9, new(5, 10)}, 
            {10, new(5, 10)}, 
        };
        public const int MaxSearches = 512;

        public int LevelNumber = 1;
        public int RoomCount = 2;

        public int MaxEnemyCount => MaxPerLevel[Game.Main.currentLevel].MaxEnemyCount;
        public int MaxObstacleCount => MaxPerLevel[Game.Main.currentLevel].MaxObstacleCount;

        public Room StartRoom = null;
        public Room EndRoom = null;

        public event Action OnDoneGenerate;

        [SerializeField] List<Room> Rooms = new();

        public void Initialize()
        {
            Game.Main.Player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            var roomCount = RoomsPerLevel[Game.Main.currentLevel];
            
            Game.Telemetry.Initialize();

            StartCoroutine(GenerateLevelCoroutine(roomCount));
        }

        public IEnumerator GenerateLevelCoroutine(int roomCount)
        {
            var result = Game.CA.GenerateRoomShaped(roomCount, featurize: false);
            
            Room previousRoom = null;
            for (int i = 0; i < result.Rooms.Count; i++)
            {
                var gr = result.Rooms[i];
                if (gr == null) continue;

                var newRoom = Game.Generator.InstantiateRoom(gr.x, gr.y);
                newRoom.transform.SetParent(transform);
                
                if (gr.IsStartRoom)
                {
                    newRoom.IsStartRoom = true;
                    StartRoom = newRoom;
                }
                if (gr.IsEndRoom)
                {
                    newRoom.IsEndRoom = true;
                    EndRoom = newRoom;
                }
                
                foreach (var n in gr.Neighbors)
                {
                    newRoom.SetDoorwayAs(DoorwayType.Door, n.Key, opened: true); /// key is direction
                }

                Rooms.Add(newRoom);

                /// Have a reference to the previous room if available
                if (i > 0 && Rooms.Count > 1)
                    newRoom.PreviousRoom = Rooms[i - 1];
                if (previousRoom != null)
                {
                    previousRoom.NextRoom = newRoom;
                }
                previousRoom = newRoom;
                
                newRoom.Initialize();
                if (gr.IsEndRoom)
                {
                    newRoom.AddExitStairs();
                }

                /// Special rooms (start, end) don't have features
                if (newRoom.IsStartRoom || newRoom.IsEndRoom) continue;
                
                var roomStats = Game.Generator.GenerateRoomStats(
                    new FeaturizeOptions(){
                        Algorithm = Game.Main.AlgorithmUsed,
                        Room = newRoom,
                        PlayerStats = Game.Telemetry.PlayerStats,
                        MaxEnemyCount = MaxEnemyCount,
                        MaxObstacleCount = MaxObstacleCount,
                        TargetStatus = Status.Accepted, /// only accepted rooms are included in level
                    }
                );
                
                if (Game.Main.UseTestLevel)
                    newRoom.FeaturizeTest(includeObstacles: true); /// use for testing
                else
                    newRoom.Featurize(roomStats); /// original
            }
            
            OnDoneGenerate?.Invoke();
            StartLevel();

            yield return null;
        }

        void StartLevel()
        {
            Debug.Log("Starting level...");
            Game.Main.Player.transform.position = StartRoom.Center.position;
            Game.Main.CurrentRoom = StartRoom;
            
            if (Game.Main.PlayerEquippedWeapon1 != null)
            {
                Game.Main.Player.SetEquippedWeapon1(Game.Main.PlayerEquippedWeapon1);
            }
            if (Game.Main.PlayerEquippedWeapon2 != null)
            {
                Game.Main.Player.SetEquippedWeapon2(Game.Main.PlayerEquippedWeapon2);
            }

            Game.Main.UnloadScene("LOADING");
            Game.Audio.PlayMusic("level");
        }

        public void FinishLevel()
        {
            Game.Main.currentLevel++;
            foreach (var r in Rooms)
            {
                Destroy(r.gameObject);
            }
            Rooms.Clear();
        }

        void OnApplicationQuit()
        {
            Game.Telemetry.SaveEntriesToJson();
        }
    }
}