using System;
using UnityEngine;

namespace URLG.Levels
{
    [Serializable]
    [CreateAssetMenu(fileName = "Tile", menuName = "RL/Tile")]
    public class TileData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Sprite;

        [Header("Properties")]
        public bool IsSolid;
        public bool IsIlluminable;
        public bool CastShadow;
        public bool CanShootThrough;
    }
}