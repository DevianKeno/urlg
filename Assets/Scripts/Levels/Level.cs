using System.Collections.Generic;
using UnityEngine;
using RL.Generator;

namespace RL.Levels
{
    public class Level : MonoBehaviour
    {
        List<Room> rooms = new();
        List<Vector2Int> cells = new();

        public void CreateLevel()
        {
            var ctx = "Level = start > Content > end;";
        }
    }
}