using System;
using UnityEngine;
using UnityEditor;

namespace RL.Levels
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TileData))]
    public class TileDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TileData tileData = (TileData)target;

            if (tileData.Sprite != null)
            {
                GUILayout.Space(10f);
                GUILayout.Label("Preview", EditorStyles.boldLabel);

                Rect spriteRect = tileData.Sprite.rect;
                Texture2D spriteTexture = tileData.Sprite.texture;

                float aspectRatio = spriteRect.width / spriteRect.height;
                float displayHeight = 350f;
                float displayWidth = displayHeight * aspectRatio;

                GUILayout.BeginVertical();
                Rect guiRect = GUILayoutUtility.GetRect(displayWidth, displayHeight);
                GUI.DrawTextureWithTexCoords(guiRect, spriteTexture, new Rect(
                    spriteRect.x / spriteTexture.width,
                    spriteRect.y / spriteTexture.height,
                    spriteRect.width / spriteTexture.width,
                    spriteRect.height / spriteTexture.height));
                GUILayout.EndVertical();
            }
        }
    }
}