using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Levels
{
    public enum DoorwayFacing { 
        North, South, East, West
    }

    public enum RoomWayType {
        Wall, Door
    }

    public class RoomDoor : MonoBehaviour
    {
        public bool IsOpen;
        public RoomWayType WayType;
        public DoorwayFacing DoorwayFacing;
        public GameObject WallTiles;
        public GameObject DoorTiles;

        void OnValidate()
        {
            if (Application.isPlaying) return;

            ReinitializeDoorways();
            SetDoorsOpen(IsOpen);
        }

        void ReinitializeDoorways()
        {
            if (WayType == RoomWayType.Wall)
            {
                if (WallTiles != null)
                {
                    WallTiles.SetActive(true);
                }
                if (DoorTiles != null)
                {
                    DoorTiles.SetActive(true);
                }
            }
            else if (WayType == RoomWayType.Door)
            {
                if (WallTiles != null)
                {
                    WallTiles.SetActive(false);
                }
                if (DoorTiles != null)
                {
                    DoorTiles.SetActive(true);
                }
            }
        }

        public void SetWayTypeAsDoor(bool value)
        {
            if (value)
            {
                WayType = RoomWayType.Door;
            }
            else
            {
                WayType = RoomWayType.Wall;
            }
            ReinitializeDoorways();
        }

        public void SetDoorsOpen(bool open)
        {
            if (WayType == RoomWayType.Wall)
            {
                return;
            }

            foreach (Transform child in DoorTiles.transform)
            {
                child.gameObject.SetActive(!open);
            }
        }

        public void ShutClosed()
        {
            if (WayType == RoomWayType.Wall)
            {
                return;
            }

            foreach (Transform child in DoorTiles.transform)
            {
                child.gameObject.SetActive(true);
            }
            Game.Audio.PlaySound("door_close");
        }
    }
}