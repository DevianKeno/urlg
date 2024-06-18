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
            var room = target as Room;
            base.OnInspectorGUI();
            
            GUILayout.Space(10f);            
            if (GUILayout.Button("Initialize"))
            {
                room.Initialize();
            }
            if (GUILayout.Button("Refresh"))
            {
                room.RefreshTiles();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Door Controls");
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Doors"))
                {
                    room.SetDoorsEditor(false);
                }
                if (GUILayout.Button("Close Doors"))
                {
                    room.SetDoorsEditor(true);
                }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}