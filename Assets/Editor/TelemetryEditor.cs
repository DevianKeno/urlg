using System.IO;
using UnityEditor;
using UnityEngine;

namespace RL.Levels
{
    [CustomEditor(typeof(Telemetry.Telemetry))]
    public class TelemetryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            var mg = target as Telemetry.Telemetry;
            
            GUILayout.Space(10f);
            if (GUILayout.Button("Save Telemetry Data"))
            {
                mg.SaveEntriesToJson();
            }
            if (GUILayout.Button("Open Save Folder"))
            {
                #if UNITY_EDITOR
                {
                    var savepath = Path.Combine(Application.persistentDataPath, "saves");
                    
                    if (!Directory.Exists(savepath))
                    {
                        Directory.CreateDirectory(savepath);
                    }

                    EditorUtility.RevealInFinder(savepath);
                }
                #else
                {
                    Game.Files.OpenSaveFolder();
                }
                #endif
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}