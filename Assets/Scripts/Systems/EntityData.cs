using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RL
{
    [Serializable]
    [CreateAssetMenu(fileName = "Entity Data", menuName = "Entity Data")]
    public class EntityData : ScriptableObject
    {
        public string Id;
        public AssetReference AssetReference;
    }
}