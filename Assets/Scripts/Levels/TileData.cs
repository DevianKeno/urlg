using System;
using UnityEngine;

namespace RL.Levels
{
    [Serializable]
    [CreateAssetMenu(fileName = "Tile", menuName = "RL/Tile")]
    public class TileData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Sprite;
        public bool IsSolid;
        public bool IsIlluminable;
        public bool IsShadowable;
    }
}