/*
Component Title: Entity (Base)
Last updated: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component is the base class for all entities in the game.

Data Structures:
    Vector3: represents the position of the entity in space
    Quaternion: represents the rotation of the entity
*/

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