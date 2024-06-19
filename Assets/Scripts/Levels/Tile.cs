using RL.Levels;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RL.Levels
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] Levels.TileData tileData;
        public Levels.TileData Data => tileData;
        public bool SnapToGrid = true;
        public Vector2 Pivot;
        public Vector2Int Coordinates;
        Levels.TileData previousTileData;
        
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] SortingGroup sortingGroup;
        [SerializeField] ShadowCaster2D shadowCaster2D;
        [SerializeField] BoxCollider2D coll;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sortingGroup = GetComponent<SortingGroup>();
            shadowCaster2D = GetComponent<ShadowCaster2D>();
        }

        void Start()
        {
            Initialize();
        }

        void OnValidate()
        {
            if (tileData != previousTileData)
            {
                Initialize();
            }
        }

        public void SetData(Levels.TileData data)
        {
            tileData = data;
        }

        public void Initialize()
        {            
            spriteRenderer = GetComponent<SpriteRenderer>();
            sortingGroup = GetComponent<SortingGroup>();
            shadowCaster2D = GetComponent<ShadowCaster2D>();

            PositionToCoordinate();
            spriteRenderer.sprite = tileData.Sprite;
            coll.enabled = tileData.IsSolid;
            if (tileData.IsIlluminable)
            {
                sortingGroup.sortingLayerID = SortingLayer.NameToID("Tiles Back");
            } else
            {
                sortingGroup.sortingLayerID = SortingLayer.NameToID("No Light");
            }
            shadowCaster2D.enabled = tileData.IsSolid;
            previousTileData = tileData;
        }

        public void Refresh()
        {
            Initialize();
        }

        public void PositionToCoordinate()
        {
            var pos = transform.position;
            Vector2Int coords = new(
                (int) pos.x,
                (int) pos.y
            );
            Coordinates = coords;
        }

        public void CoordinateToPosition(Vector2Int value)
        {
            Coordinates = value;
            transform.position = new()
            {
                x = value.x - Pivot.x,
                y = value.y - Pivot.y
            };
        }
    }
}