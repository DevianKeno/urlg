using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static URLG.Generator.Generator.Map;

namespace URLG.CellularAutomata
{
    /// <summary>
    /// Room component.
    /// </summary>
    public class MockRoom : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int Coordinates { get; set; }
        public int x => Coordinates.x;
        public int y => Coordinates.y;
        public bool IsTyped = false;
        // public RoomType Type;

        Dictionary<Cardinal, MockRoom> neighbors = new();
        public Dictionary<Cardinal, MockRoom> Neighbors => neighbors;
        Color _originalColor;

        [Header("Doors")]
        public bool North;
        public bool South;
        public bool East;
        public bool West;

        [SerializeField] GameObject doorNorth;
        [SerializeField] GameObject doorSouth;
        [SerializeField] GameObject doorEast;
        [SerializeField] GameObject doorWest;

        [SerializeField] SpriteRenderer spriteRenderer;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                doorNorth?.SetActive(North);
                doorSouth?.SetActive(South);
                doorEast?.SetActive(East);
                doorWest?.SetActive(West);
            }
        }

        void Start()
        {
            _originalColor = spriteRenderer.color;
        }

        /// <summary>
        /// Connects the doorways of this Room to the given Room.
        /// </summary>
        public void Connect(MockRoom other)
        {
            if (other != null)
            {
                Neighbors.Add(DirectionTo(other), other);
                other.Neighbors.Add(other.DirectionTo(this), this);
            }
        }

        /// <summary>
        /// Gets the cardinal direction of the given Room <b>from</b> this Room.
        /// </summary>
        public Cardinal DirectionTo(MockRoom other)
        {
            int dx = other.x - x;
            int dy = other.y - y;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return dx > 0 ? Cardinal.East : Cardinal.West;
            return dy > 0 ? Cardinal.North : Cardinal.South;
        }

        public void ToggleDoorway(Cardinal cardinal, bool isOpen)
        {
            switch (cardinal)
            {
                case Cardinal.North:
                {
                    North = isOpen;
                    doorNorth?.SetActive(isOpen);
                    break;
                }
                case Cardinal.South:
                {
                    South = isOpen;
                    doorSouth?.SetActive(isOpen);
                    break;
                }
                case Cardinal.East:
                {
                    East = isOpen;
                    doorEast?.SetActive(isOpen);
                    break;
                }
                case Cardinal.West:
                {
                    West = isOpen;
                    doorWest?.SetActive(isOpen);
                    break;
                }
            }
        }

        public MockRoom GetNeighbor(Cardinal cardinal)
        {
            neighbors.TryGetValue(cardinal, out MockRoom neighbor);
            return neighbor;
        }

        public void OnMouseEnter()
        {
            LeanTween.cancel(gameObject);

            var color = spriteRenderer.color;
            color.a = 0.66f;
            spriteRenderer.color = color;
        }

        public void OnMouseDown()
        {
            var color = spriteRenderer.color;
            color.a = 0.33f;
            spriteRenderer.color = color;
            Invoke(nameof(ResetColor), 0.1f);
        }

        public void OnMouseExit()
        {
            ResetColor();
        }

        void ResetColor()
        {
            spriteRenderer.color = _originalColor;
        }
    }
}