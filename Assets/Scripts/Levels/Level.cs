using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using URLG.CellularAutomata;
using static URLG.Generator.Generator.Map;

namespace URLG.Levels
{
    public class Level : MonoBehaviour
    {
        public int RoomCount = 1;
        public List<Room> Rooms = new();

        void Start()
        {
            GenerateLevel();
        }

        public void GenerateLevel()
        {
            var settings = new NoiseGridSettings()
            {
                Width = 48,
                Height = 32,
                Density = 0,
            };

            var generatedRooms = Game.CA.GenerateRooms(RoomCount);

            foreach (var gr in generatedRooms)
            {
                var newRoom = Game.Generator.InstantiateRoom(gr.x, gr.y);
                
                foreach (var n in gr.Neighbors)
                {
                    var direction = n.Key;
                    var door = newRoom.GetDoor(direction);
                    door.DoorwayType = DoorwayType.Door;
                    door.Open();
                }
                
                Rooms.Add(newRoom);
            }
        }
    }
}