/*

Component Title: Tile Data
Data written: July 6, 2024
Date revised: October 26, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Data structure for tile data, storing all information associated with it.

Data Structures/Key Variables:
    [Definitions are found at their respective declarations]
*/

using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RL.Levels
{
    /// <summary>
    /// Data structure for tile data, storing all information associated with it.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Tile", menuName = "RL/Tile")]
    public class TileData : ScriptableObject
    {
        /// <summary>
        /// Unique identifier representing this particular tile.
        /// </summary>
        public string Id;
        /// <summary>
        /// Proper display name of this tile. 
        /// </summary>
        public string Name;
        /// <summary>
        /// The sprite texture of this tile.
        /// </summary>
        public Sprite Sprite;
        /// <summary>
        /// Addressable asset reference.
        /// </summary>
        public AssetReference AssetReference;

        [Header("Properties")]
        /// <summary>
        /// Whether if the entities can collide with this tile.
        /// </summary>
        public bool IsSolid;
        /// <summary>
        /// Whether if this tile can be illuminated by light sources.
        /// </summary>
        public bool IsIlluminable;
        /// <summary>
        /// Whether if this tile casts a shadow when illuminated by light sources.
        /// </summary>
        public bool CastShadow;
        /// <summary>
        /// Whether if projectiles pass through this tile.
        /// </summary>
        public bool CanShootThrough;
    }
}