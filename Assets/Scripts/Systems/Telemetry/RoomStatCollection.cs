using System;

using RL.RD;

namespace RL.Telemetry
{
    [Serializable]
    public class RoomStatCollection : StatCollection
    {
        public int Seed;
        public int Classification;
        public int TotalEnemyCount => this[StatKey.EnemyCountFire].Value + this[StatKey.EnemyCountBeam].Value + this[StatKey.EnemyCountWave].Value;
        public int TotalObstacleCount => this[StatKey.ObstacleCountFire].Value + this[StatKey.ObstacleCountBeam].Value + this[StatKey.ObstacleCountWave].Value;
        
        public RoomStatCollection(StatKey[] stats) : base(stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public static RoomStatCollection FromAREntry(ARDataEntry entry)
        {
            var stats = new RoomStatCollection(Telemetry.RoomStatsKeys);

            foreach (var key in Telemetry.RoomStatsKeys)
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