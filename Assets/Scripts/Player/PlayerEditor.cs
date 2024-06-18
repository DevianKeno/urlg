using UnityEngine;
using UnityEditor;

namespace RL.Player
{
    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            var player = (PlayerController) target;

            if (GUILayout.Button("Save Data"))
            {
                player.SaveStats();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}