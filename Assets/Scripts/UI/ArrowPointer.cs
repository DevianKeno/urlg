using UnityEngine;

namespace RL.UI
{
    public class ArrowPointer : MonoBehaviour
    {
        public bool IsEnabled = true;

        void Update()
        {
            if (IsEnabled && Game.Main.CurrentRoom.HasNextRoom)
            {
                var a = Game.Main.Player.transform.position;
                Vector3 b;
                if (Game.Main.CurrentRoom.NextRoom.IsEndRoom && Game.Main.CurrentRoom.NextRoom.HasStairs)
                {
                    b = Game.Main.CurrentRoom.NextRoom.Stairs.transform.position;
                }
                else
                {
                    b = Game.Main.CurrentRoom.NextRoom.Center.position;
                }
                RotateArrow(a, b);
            }
        }
        
        public void RotateArrow(Vector3 pointA, Vector3 pointB)
        {
            Vector3 direction = pointB - pointA;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
}