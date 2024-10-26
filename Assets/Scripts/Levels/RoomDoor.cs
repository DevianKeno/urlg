using UnityEngine;
using static RL.Generator.Generator.Map;

namespace RL.Levels
{
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
        }

        public void Open()
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(false);
            this.IsOpen = true;
        }

        public void Close()
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(true);
            this.IsOpen = false;
        }

        public void SetDoorsOpen(bool isOpen)
        {
            if (DoorwayType == DoorwayType.Wall)
            {
                return;
            }

            BarsTiles?.SetActive(!isOpen);
            this.IsOpen = isOpen;
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
            this.IsOpen = false;

            Game.Audio.Play("door_close");
        }
    }
}