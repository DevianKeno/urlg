/*
*   Program Title: Player Stat Collection (Data Structure)
*   Last updated: October 11, 2024
*   
*   Programmers:
*       Gian Paolo Buenconsejo
*   
*   Purpose:
*       A subclass of StatCollection, designed specifically to store a collection of gameplay statistics for the Player.
*
*   Data Structures:
*       Dictionary: to store a StatKey and the actual represented Stat in a key-value pair in a collection.
*       List: used for viewing a read-only list of the Stats within the collection.
*/


using System;

using RL.RD;

namespace RL.Telemetry
{
    [Serializable]
    public class PlayerStatCollection : StatCollection
    {
        public int Seed;
        public int TotalHitCount => this[StatKey.HitCountFire].Value + this[StatKey.HitCountBeam].Value + this[StatKey.HitCountWave].Value;
        public int TotalUseCount => this[StatKey.UseCountFire].Value + this[StatKey.UseCountBeam].Value + this[StatKey.UseCountWave].Value;
        
        public PlayerStatCollection(StatKey[] stats) : base(stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        /// <summary>
        /// Constructs 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>The constructed PlayerStatCollection</returns>
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