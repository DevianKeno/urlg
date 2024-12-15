using UnityEngine;

namespace RL.Projectiles
{
    public enum WeaponType { Fireball, Beam, Wave }

    [CreateAssetMenu(fileName = "Projectile", menuName = "Roguelike/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        public WeaponType Type;
        public string Name;
        public Sprite Sprite;
        public float Speed;
        public float Damage;
        public float DespawnAfter;
        public GameObject Object;
    }
}