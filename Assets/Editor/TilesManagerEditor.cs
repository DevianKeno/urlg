using UnityEngine;
using UnityEditor;

namespace RL.Systems
{
    [CustomEditor(typeof(TilesManager))]
    public class TilesManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var tm = target as TilesManager;
            base.OnInspectorGUI();
            
            GUILayout.Space(10f);
            if (GUILayout.Button("Initialize"))
            {
                tm.Initialize();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}