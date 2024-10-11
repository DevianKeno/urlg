using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using RL.Entities;

namespace RL
{
    public class EntityManager : MonoBehaviour
    {
        public List<EntityData> EntityList = new();
        Dictionary<string, EntityData> _entityDict = new();
        
        public void Initialize()
        {
            foreach (var data in Resources.LoadAll<EntityData>("Data/Entities"))
            {
                _entityDict[data.Id] = data;
                EntityList.Add(data);
            }
        }

        public void Spawn(string id, Action<Entity> onSpawn = null)
        {
            if (_entityDict.ContainsKey(id))
            {
                Addressables.LoadAssetAsync<GameObject>(_entityDict[id].AssetReference).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result);
                        if (go.TryGetComponent(out Entity entity))
                        {
                            onSpawn?.Invoke(entity);
                            return;
                        }
                        Destroy(go);
                    }
                };
            }
        }
    }
}