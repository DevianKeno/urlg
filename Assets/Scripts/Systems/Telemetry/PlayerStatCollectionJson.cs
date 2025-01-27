using System;

using RL.RD;

namespace RL.Telemetry
{
    [Serializable]
    public class PlayerStatCollectionJson : StatCollectionJson
    {
        public int Seed;

        public PlayerStatCollectionJson(int[] stats) : base(stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public static PlayerStatCollection FromAREntry(ARDataEntry entry)
        {
            var stats = new PlayerStatCollection(Telemetry.PlayerStatsKeys);

            foreach (var key in Telemetry.PlayerStatsKeys)
            {
                if (entry.Values.TryGetValue(key, out var value))
                {
                    stats.GetStat(key).Value = value;
                }
            }

            return stats;
        }
    }
}