using UnityEditor;
using UnityEngine;

namespace RL.Levels
{
    [CustomEditor(typeof(Room))]
    public class RoomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            var room = target as Room;
            
            GUILayout.Space(10f);
            if (GUILayout.Button("Initialize"))
            {
                room.Initialize();
            }

            // GUILayout.Space(10f);
            // GUILayout.Label("Door Controls");
            // GUILayout.BeginHorizontal();
            //     if (GUILayout.Button("Open Doors"))
            //     {
            //         room.SetDoorsEditor(true);
            //     }
            //     if (GUILayout.Button("Close Doors"))
            //     {
            //         room.SetDoorsEditor(false);
            //     }
            // GUILayout.EndHorizontal();
            
            // GUILayout.Space(10f);
            // GUILayout.Label("Generate Randomized");
            // GUILayout.BeginHorizontal();
            //     if (GUILayout.Button("Enemies"))
            //     {
            //         room.GenerateEnemiesRandomEditor();
            //     }
            //     if (GUILayout.Button("Obstacles"))
            //     {
            //         room.GenerateObstaclesRandomEditor();
            //     }
            // GUILayout.EndHorizontal();

            // if (GUILayout.Button("Generate Features"))
            // {
            //     room.GenerateFeaturesRandom();
            // }

            serializedObject.ApplyModifiedProperties();
        }
    }
}