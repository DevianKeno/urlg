using System;
using System.Collections.Generic;
using System.Linq;

namespace RL.Telemetry
{
    [Serializable]
    public class PlayerStatCollection : StatCollection
    {
        public int TotalHitCount => this[StatKey.HitCountFire].Value + this[StatKey.HitCountBeam].Value + this[StatKey.HitCountWave].Value;
        public int TotalUseCount => this[StatKey.UseCountFire].Value + this[StatKey.UseCountBeam].Value + this[StatKey.UseCountWave].Value;
        
        public PlayerStatCollection(StatKey[] stats) : base(stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }
    }
}