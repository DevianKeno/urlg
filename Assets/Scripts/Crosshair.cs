using UnityEngine;

namespace URLG
{
    public class Crosshair : MonoBehaviour
    {
        public Camera cam;
        public Texture2D Texture;

        void Start()
        {
            var hotspot = new Vector2(Texture.width / 2f, Texture.height / 2f);
            Cursor.SetCursor(Texture, hotspot, CursorMode.Auto);
        }

        void Update()
        {            
            // var target = cam.ScreenToWorldPoint(Input.mousePosition);
            // target.z = 0f;
            // transform.position = target;
        }

        void OnDestroy()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}