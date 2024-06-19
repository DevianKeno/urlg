using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RL.Enemies;

public enum WeaponType { Fireball, Beam, Wave }

namespace RL.Levels
{
    public struct RoomData
    {
        public int EnemyCountFire;
        public int EnemyCountBeam;
        public int EnemyCountWave;
        public int ObstacleCountFire;
        public int ObstacleCountBeam;
        public int ObstacleCountWave;
    }

    public class Room : MonoBehaviour
    {
        public List<GameObject> Layers = new();
        public List<Tile> Tiles = new();
        public bool HasNorthDoor = true;
        public bool HasSouthDoor = true;
        public bool HasEastDoor = true;
        public bool HasWestDoor = true;

        [SerializeField] GameObject enemiesContainer;
        
        [Header("Objects")]
        [SerializeField] RoomDoor northDoor;
        [SerializeField] RoomDoor southDoor;
        [SerializeField] RoomDoor eastDoor;
        [SerializeField] RoomDoor westDoor;

        public int EnemyCount
        {
            get => 0;
        }
        public int ObstacleCount
        {
            get => 0;
        }
        public int TargetCount => EnemyCount + ObstacleCount;

        public void Initialize()
        {
            Layers = new();
            Tiles = new();
            for (int i = 0; i < transform.childCount; i++)
            {
                var go = transform.GetChild(i).gameObject;
                Layers.Add(transform.GetChild(i).gameObject);

                if (go.TryGetComponent(out RoomTileLayer rtl))
                {
                    go.name = $"Layer {i} ({rtl.Name})";
                }
                
                foreach (Transform t in go.transform)
                {
                    if (t.TryGetComponent(out Tile tile))
                    {
                        tile.name = "Tile";
                        tile.Initialize();
                        tile.GetComponent<SortingGroup>().sortingOrder = i;
                        Tiles.Add(tile);
                    }
                }
            }
            northDoor.SetAsDoor(HasNorthDoor);
            southDoor.SetAsDoor(HasSouthDoor);
            eastDoor.SetAsDoor(HasEastDoor);
            westDoor.SetAsDoor(HasWestDoor);
        }

        public void ShutDoors()
        {
            StartCoroutine(nameof(ShutDoorsCoroutine));
        }

        IEnumerator ShutDoorsCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            if (HasNorthDoor)
            {
                northDoor.SetDoors(HasNorthDoor);
            }
            if (HasSouthDoor)
            {
                southDoor.SetDoors(HasSouthDoor);
            }
            if (HasEastDoor)
            {
                eastDoor.SetDoors(HasEastDoor);
            }
            if (HasWestDoor)
            {
                westDoor.SetDoors(HasWestDoor);
            }
        }

        public void RefreshTiles()
        {
            foreach (Tile t in Tiles)
            {
                t.Refresh();
            }
            northDoor.SetDoors(HasNorthDoor);
            southDoor.SetDoors(HasSouthDoor);
            eastDoor.SetDoors(HasEastDoor);
            westDoor.SetDoors(HasWestDoor);
        }

        public void CountFeatures()
        {
            foreach (Transform c in enemiesContainer.transform)
            {
                if (c.TryGetComponent(out Enemy enemy))
                {
                    if (enemy is FireWeak)
                    {
                        // Game.Telemetry.RoomStats["enemyCountFire"].Increment();
                    }
                    else if (enemy is BeamWeak)
                    {
                        // Game.Telemetry.RoomStats["enemyCountBeam"].Increment();
                    }
                    else if (enemy is WaveWeak)
                    {
                        Game.Telemetry.RoomStats["enemyCountWave"].Increment();
                    }
                }
            }
        }

        #region Editor
        
        public void SetDoorsEditor(bool open)
        {
            northDoor?.SetDoors(open);
            southDoor?.SetDoors(open);
            eastDoor?.SetDoors(open);
            westDoor?.SetDoors(open);
        }

        #endregion
    }
}