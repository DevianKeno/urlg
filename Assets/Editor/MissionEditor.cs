using UnityEditor;
using UnityEngine;

using URLG.Generator;

namespace URLG.Levels
{
    [CustomEditor(typeof(MissionGenerator))]
    public class MissionGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            var mg = target as MissionGenerator;
            
            GUILayout.Space(10f);
            if (GUILayout.Button("Generate Mission"))
            {
                mg.GenerateMission();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}