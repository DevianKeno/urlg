using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RL.Levels
{
    [Serializable]
    [CreateAssetMenu(fileName = "Tile", menuName = "RL/Tile")]
    public class TileData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Sprite;
        public AssetReference AssetReference;

        [Header("Properties")]
        public bool IsSolid;
        public bool IsIlluminable;
        public bool CastShadow;
        public bool CanShootThrough;
    }
}