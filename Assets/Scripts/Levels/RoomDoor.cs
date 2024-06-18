using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Levels
{
    public class RoomDoor : MonoBehaviour
    {
        [SerializeField] GameObject wall;
        [SerializeField] GameObject bars;
        public void SetDoors(bool open)
        {
            foreach(Transform c in transform)
            {
                c.gameObject.SetActive(open);
            }
        }

        public void SetAsDoor(bool value)
        {
            if (value)
            {
                wall.SetActive(false);
                bars.SetActive(true);
            } else
            {
                wall.SetActive(true);
                bars.SetActive(false);
            }
        }
    }
}