using UnityEngine;

namespace RL.Entities
{
    public class Entity : MonoBehaviour
    {
        [SerializeField] protected EntityData data;
        public EntityData Data => data;
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        public Vector3 LocalPosition
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }
        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }
    }
}