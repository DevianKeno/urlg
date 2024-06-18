using UnityEditor;
using UnityEngine;

namespace RL.Generator
{
    [CustomEditor(typeof(Generator))]
    public class GeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var generator = target as Generator;
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Create Tile"))
            {
                generator.CreateTile(generator.TileId, generator.Cell);
            }

            if (GUILayout.Button("Create Room"))
            {
                generator.CreateRoom(generator.GeneratorParams);
            }
            if (GUILayout.Button("Refresh Data"))
            {
                generator.Refresh();
            }            

            serializedObject.ApplyModifiedProperties();
        }
    }
}