using System;
using System.Collections.Generic;
using System.Linq;
using RL.RD;

namespace RL.Telemetry
{
    [Serializable]
    public class RoomStatCollectionJson : StatCollectionJson
    {
        public int Seed;
        public int Classification;

        public RoomStatCollectionJson(int[] stats) : base(stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }
    }
}