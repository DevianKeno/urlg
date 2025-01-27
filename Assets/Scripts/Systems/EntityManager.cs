/*

Component Title: Entity Manager
Data written: October 9, 2024
Date revised: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component manages the spawning/killing of all the game's entities.
    Contains methods necessary for handling entities in the game
        e.g., Spawn()

Data Structures:
    Dictionary: used to store the loaded data of Entities for the game
        Key is the Id of the entity; Value is the data of the Entity itself.
    List: used to store the entity data(s) as a read-only list
*/

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