using System;
using UnityEngine;

namespace RL.Player
{
    [Serializable]
    public struct PlayerStats
    {
        public int UseCountFire;
        public int UseCountBeam;
        public int UseCountWave;
        public int TotalUseCount => UseCountFire + UseCountBeam + UseCountWave;
        public int HitCountFire;
        public int HitCountLaser;
        public int HitCountWave;
        public int TotalHitCount => HitCountFire + HitCountLaser + HitCountWave;
        public int HitsTaken;
    }

    public struct SessionStats
    {
        public int EnemyAttackCount;
    }
}