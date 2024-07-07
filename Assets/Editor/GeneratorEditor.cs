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

            if (GUILayout.Button("Refresh Data"))
            {
                
            }            

            serializedObject.ApplyModifiedProperties();
        }
    }
}