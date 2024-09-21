using UnityEngine;
using URLG.Projectiles;

namespace URLG.Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Roguelike/Weapon")]
    public class Weapon : ScriptableObject
    {
        public string Name;
        public Sprite Sprite;
        public float Damage;
        public float FireRate;
        public ProjectileData ProjectileData;
    }
}