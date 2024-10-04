using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static RL.Generator.Generator.Map;

namespace RL.Levels
{
    public enum DoorwayFacing { 
        North, South, East, West
    }

    public enum DoorwayType {
        Wall, Door
    }

    public class RoomDoor : MonoBehaviour
    {
        public bool IsOpen;

        DoorwayType doorwayType;
        public DoorwayType DoorwayType
        {
            get
            {
                return doorwayType;
            }
            set
            {
                doorwayType = value;
                ReinitializeDoorway();
            }
        }
        public Cardinal Direction;

        public GameObject WallTiles;
        public GameObject DoorTiles;
        public GameObject BarsTiles;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                ReinitializeDoorway();
                SetDoorsOpen(IsOpen);
            }
        }

        public void ReinitializeDoorway()
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                DoorTiles?.SetActive(false);

                WallTiles?.SetActive(true);
                
            }
            else if (DoorwayType == DoorwayType.Door)
            {
                WallTiles?.SetActive(false);

                DoorTiles?.SetActive(true);
            }
            
            SetDoorsOpen(IsOpen);
        }

        public void Open()
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(false);
        }

        public void Close()
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(true);
        }

        public void SetDoorsOpen(bool isOpen)
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(!isOpen);
        }

        public void ShutClosed()
        {
            if (DoorwayType == DoorwayType.Wall)
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