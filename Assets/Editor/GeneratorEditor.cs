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
            base.OnInspectorGUI();

            var generator = target as Generator;

            if (GUILayout.Button("Refresh Data"))
            {
                
            }
            if (GUILayout.Button("Refresh Data"))
            {
                
            }            

            serializedObject.ApplyModifiedProperties();
        }
    }
}