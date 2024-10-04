using UnityEditor;
using UnityEngine;
using RL.CellularAutomata;

namespace RL.UnityEditor
{
    [CustomEditor(typeof(CellularAutomataHelper))]
    public class CellularAutomataHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            var caHelper = target as CellularAutomataHelper;
            
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Noise Grid", EditorStyles.boldLabel);
            if (GUILayout.Button("Generate Noise Grid"))
            {
                caHelper.GenerateNoiseGrid();
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Iterations", EditorStyles.boldLabel);
                if (GUILayout.Button("Increment"))
                {
                    caHelper.IncrementIteration();
                }
                if (GUILayout.Button("Decrement"))
                {
                    caHelper.DecrementIteration();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            if (GUILayout.Button("Generate Mock Level"))
            {
                caHelper.GenerateRooms(caHelper.RoomCount);
            }
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Grid All"))
            {
                caHelper.RefreshGridAll();
            }
            if (GUILayout.Button("Destroy All Rooms"))
            {
                caHelper.DestroyAllRooms();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}