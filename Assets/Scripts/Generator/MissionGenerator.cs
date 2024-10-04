using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Generator
{
    public enum RoomType {
        Start, End, Room, Key, Lock
    }

    public class Mission
    {
        
    }

    public class MissionGenerator : MonoBehaviour
    {
        const string startToken = "START";
        const string endToken = "END";
        const string roomToken = "ROOM";
        const string levelToken = "level";

        [Header("Settings")]
        public int RoomCount = 4;
        public bool RandomizeRoomCount = false;
        public int MinRooms = 3;
        public int MaxRooms = 6;

        public string GenerateMission()
        {
            if (RandomizeRoomCount)
            {
                RoomCount = UnityEngine.Random.Range(MinRooms, MaxRooms + 1);
            }

            string mission = levelToken + " -> " + startToken;

            for (int i = 0; i < RoomCount; i++)
            {
                mission += " " + roomToken;
            }

            mission += " " + endToken;
            print(mission);
            return mission;
        }
    }
}