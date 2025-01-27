/*

Component Title: Stat Collection Json
Data written: September 21, 2024
Date revised: October 15, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    

Data Structures:
    Dictionary: used to store the Stat list along with its index when saved to CSV format
        Key is the index of the stat when saved to CSV,  Value is the Stat data itself
*/

using System;
using System.Collections.Generic;

namespace RL.Telemetry
{
    [Serializable]
    public class StatCollectionJson
    {
        protected Dictionary<int, Stat> _statList = new();

        public StatCollectionJson(int[] stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public Stat this[int key]
        {
            get
            {
                TryGetStat(key, out var stat);
                return stat;
            }
        }
        
        public Stat GetStat(int key)
        {
            if (_statList.TryGetValue(key, out var stat))
            {
                return stat;
            }
            return new Stat(key, 0);
        }

        public bool TryGetStat(int key, out Stat stat)
        {
            if (_statList.TryGetValue(key, out var gotStat))
            {
                stat = gotStat;
                return true;
            }
            else
            {
                stat = null;
                return false;
            }
        }

        public List<StatSaveData> SaveToJson()
        {
            var list = new List<StatSaveData>();

            foreach (var stat in _statList)
            {
                list.Add(stat.Value.SaveToJson());
            }
            
            return list;
        }

        internal void Reset()
        {
            foreach (var s in _statList.Values)
            {
                var stat = _statList[(int) s.key];
                stat.Value = 0;
            }
        }
    }
}