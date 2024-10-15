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
        public static Dictionary<int, int> RoomsPerLevel = new()
        {
            {1, 2}, {2, 2}, {3, 4}, {4, 4}, {5, 6}, {6, 2}, 
        };
        public static Dictionary<int, RoomMaxParameters> MaxPerLevel = new()
        {
            {1, new(3, 3)},
            {2, new(3, 6)},
            {3, new(4, 9)},
            {4, new(6, 12)},
            {5, new(6, 12)},
            {6, new(9, 15)}, 
        };
        public const int MaxSearches = 512;

        public int LevelNumber = 1;
        public int RoomCount = 2;
        [Range(0, 100)] public int RejectedRoomsThreshold = 25;

        public int MaxEnemyCount => MaxPerLevel[LevelNumber].MaxEnemyCount;
        public int MaxObstacleCount => MaxPerLevel[LevelNumber].MaxObstacleCount;

        public Room StartRoom = null;
        public Room EndRoom = null;

        public event Action OnDoneGenerate;

        [SerializeField] List<Room> Rooms = new();

        public void Initialize()
        {
            Game.Main.Player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            var roomCount = RoomsPerLevel[Game.Main.currentLevel];
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
                
                Status targetStatus;
                if (UnityEngine.Random.Range(0, 100) > RejectedRoomsThreshold)
                    targetStatus = Status.Accepted;
                else
                    targetStatus = Status.Rejected;

                var roomStats = Game.Generator.GenerateRoomStats(
                    new FeaturizeOptions(){
                        Algorithm = Game.Main.AlgorithmUsed,
                        Room = newRoom,
                        PlayerStats = Game.Telemetry.PlayerStats,
                        MaxEnemyCount = MaxEnemyCount,
                        MaxObstacleCount = MaxObstacleCount,
                        TargetStatus = targetStatus,
                    }
                );
                
                // newRoom.Featurize(roomStats); /// original
                 newRoom.FeaturizeTest(); /// use for testing
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
            
            Game.Main.UnloadScene("LOADING");
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