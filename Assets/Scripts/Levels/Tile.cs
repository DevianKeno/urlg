/*

Component Title: Tile (Base)
Data written: July 6, 2024
Date revised: October 26, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Base class to represent tiles in the game.

Data Structures:
    [Definitions are found at their respective declarations]
*/

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RL.Levels
{
    /// <summary>
    /// Base class to represent tiles in the game.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [SerializeField] TileData tileData;
        /// <summary>
        /// The data representing this tile.
        /// </summary>
        public TileData TileData
        {
            get
            {
                return tileData;
            }
            set
            {
                tileData = value;
            }
        }
        Vector2Int coordinates;
        /// <summary>
        /// The position of this tile in cell grid coordinates, relative to world space.
        /// </summary>
        public Vector2Int Coordinates
        {
            get
            {
                return coordinates;
            }
            set
            {
                coordinates = value;
            }
        }
        Vector2Int localCoordinates;
        /// <summary>
        /// The position of this tile in cell grid coordinates, relative to the room its in.
        /// </summary>
        public Vector2Int LocalCoordinates
        {
            get
            {
                return localCoordinates;
            }
            set
            {
                localCoordinates = value;
            }
        }
        public int foregroundLayerID;

        TileData _previousTileData;
        
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected BoxCollider2D coll;
        [SerializeField] protected ShadowCaster2D shadowCaster2D;
        

        #region Initializing methods

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            shadowCaster2D = GetComponent<ShadowCaster2D>();
        }

        void Start()
        {
            Initialize();
        }

        void InitializeComponents()
        {
            if (TryGetComponent<SpriteRenderer>(out var a))
            {
                spriteRenderer = a;
            }
            
            if (TryGetComponent<ShadowCaster2D>(out var c))
            {
                shadowCaster2D = c;
            }
        }

        public void Initialize()
        {
            gameObject.name = $"Tile ({Coordinates.x}, {Coordinates.y}) ({tileData.Name})";
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = tileData.Sprite;
                
                if (tileData.IsIlluminable)
                {
                    spriteRenderer.sharedMaterial = Resources.Load<Material>("Materials/sprite_lit");
                } else
                {
                    spriteRenderer.sharedMaterial = Resources.Load<Material>("Materials/sprite_unlit");
                }
            };

            if (shadowCaster2D != null)
            {
                shadowCaster2D.enabled = tileData.CastShadow;
            }

            if (coll != null)
            {
                coll.enabled = true;
                coll.isTrigger = !tileData.IsSolid;
            }

            _previousTileData = tileData;
            PositionToCoordinate();
        }

        #endregion


        void OnValidate()
        {
            if (Application.isPlaying) return;
            
            if (tileData != _previousTileData)
            {
                InitializeComponents();
                Initialize();
            }
        }


        #region Public methods

        /// <summary>
        /// Updates its coordinate position from its transform position.
        /// </summary>
        public void PositionToCoordinate()
        {
            var pos = new Vector3(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y),
                Mathf.Round(transform.position.z)
            );
            Vector2Int coords = new(
                (int) pos.x,
                (int) pos.y
            );
            Coordinates = coords;
            localCoordinates = new Vector2Int((int) transform.localPosition.x, (int) transform.localPosition.y);
        }

        /// <summary>
        /// Updates its transform position from its coordinate position.
        /// </summary>
        public void CoordinateToPosition(Vector2Int coords)
        {
            transform.position = new()
            {
                x = coords.x,
                y = coords.y
            };
            Coordinates = coords;
        }

        /// <summary>
        /// Updates its transform local position from its coordinate position.
        /// </summary>
        public void CoordinateToLocalPosition(Vector2Int coords)
        {
            transform.localPosition = new()
            {
                x = coords.x,
                y = coords.y
            };
            localCoordinates = coords;
        }

        /// <summary>
        /// Changes the data associated with this tile given a valid id.
        /// </summary>
        public void SetTileDataFromId(string id)
        {
            var newData = Game.Tiles.GetTileDataFromId(id);
            if (newData != null)
            {
                tileData = newData;
                Initialize();
            }
        }

        #endregion
    }
}