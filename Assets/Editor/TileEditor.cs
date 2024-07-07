using UnityEditor;
using UnityEngine;

namespace RL.Levels
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var tile = target as Tile;
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Initialize"))
            {
                tile.Initialize();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}