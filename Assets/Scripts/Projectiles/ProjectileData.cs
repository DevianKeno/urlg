using UnityEngine;

namespace RL.Projectiles
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Roguelike/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        public string Name;
        public Sprite Sprite;
        public float Speed;
        public float Damage;
        public float DespawnAfter;
        public GameObject Object;
    }
}