using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RL.Levels
{
    [Serializable]
    [CreateAssetMenu(fileName = "Obstacle", menuName = "RL/Obstacle")]
    public class ObstacleData : ScriptableObject
    {
        public string Id;
        public string Name;
        public AssetReference AssetReference;

        [Header("Properties")]
        public bool IsSolid;
        public bool IsIlluminable;
        public bool CastShadow;
        public bool CanShootThrough;
    }
}