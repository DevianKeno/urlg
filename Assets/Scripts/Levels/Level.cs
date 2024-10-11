using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using RL.CellularAutomata;
using static RL.Generator.Generator.Map;
using RL.UI;
using RL.Classifiers;
using RL.Player;

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
            {1, new(1, 3)},
            {2, new(3, 6)},
            {3, new(4, 9)},
            {4, new(6, 12)},
            {5, new(6, 12)},
            {6, new(9, 15)}, 
        };
        public const int MaxSearches = 512;

        public int LevelNumber = 1;
        public int RoomCount = 2;

        public Room StartRoom = null;
        public Room EndRoom = null;

        [SerializeField] List<Room> Rooms = new();

        public void Initialize()
        {
            Game.Main.Player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            var roomCount = RoomsPerLevel[Game.Main.currentLevel];
            GenerateLevel(roomCount);
        }

        public void GenerateLevel(int roomCount)
        {
            var generatedRooms = Game.CA.GenerateRooms(roomCount);
            
            Room previousRoom = null;
            for (int i = 0; i < generatedRooms.Count; i++)
            {
                var gr = generatedRooms[i];
                if (gr == null) continue;

                var newRoom = Game.Generator.InstantiateRoom(gr.x, gr.y);
                
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

                if (i > 0 && Rooms.Count > 1)
                    newRoom.PreviousRoom = Rooms[i - 1];
                
                if (previousRoom != null)
                {
                    previousRoom.NextRoom = newRoom;
                }
                previousRoom = newRoom;
                
                newRoom.Initialize();

                if (newRoom.IsStartRoom || newRoom.IsEndRoom) continue;
                
                /// Featurize
                IResult result = null;
                var roomStats = RDTelemetryUI.ConstructRoomRandom(
                    MaxPerLevel[Game.Main.currentLevel].MaxEnemyCount,
                    MaxPerLevel[Game.Main.currentLevel].MaxObstacleCount);
                
                newRoom.Featurize(roomStats);

                // for (int i = 0; i < MaxSearches; i++)
                // {
                //     if (Game.Main.AlgorithmUsed == PCGAlgorithm.AcceptReject)
                //     {
                //         result = ARClassifier.Classify(Game.Telemetry.PlayerStats, roomStats, 0.2f, normalized: true);
                //     }
                //     else if (Game.Main.AlgorithmUsed == PCGAlgorithm.GaussianNaiveBayes)
                //     {
                //         result = GaussianNaiveBayes.Instance.ClassifyRoom(Game.Telemetry.PlayerStats, roomStats);
                //     }

                //     if (result.Status == Status.Accepted)
                //     {
                //         newRoom.Featurize(roomStats);
                //         break;
                //     }
                //     else if (result.Status == Status.Rejected)
                //     {
                //         newRoom.Featurize(roomStats);
                //         break;
                //     }
                // }
                // // newRoom.GenerateFeaturesRandom();
            }
            
            StartLevel();
        }

        void StartLevel()
        {
            Game.Main.Player.transform.position = StartRoom.Center.position;
            Game.Main.CurrentRoom = StartRoom;
        }
    }
}