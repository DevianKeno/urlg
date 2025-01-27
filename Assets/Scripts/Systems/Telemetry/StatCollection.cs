/*
*   Program Title: Stat Collection (Data Structure)
*   Last updated: November 8, 2024
*   
*   Programmers:
*       Gian Paolo Buenconsejo
*   
*   Purpose:
*       To store a collection of gameplay statistics, enclosed in the 'Stat' class.
*
*   Data Structures:
*       Dictionary: to store a StatKey and the actual represented Stat in a key-value pair in a collection.
*       List: used for viewing a read-only list of the Stats within the collection.
*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace RL.Telemetry
{
    [Serializable]
    public class StatCollection
    {
        protected Dictionary<StatKey, Stat> _statList = new();
        public List<Stat> Stats => _statList.Values.ToList();

        public StatCollection(StatKey[] stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public Stat this[StatKey key]
        {
            get
            {
                return GetStat(key);
            }
        }
        
        public Stat GetStat(StatKey key)
        {
            if (_statList.TryGetValue(key, out var stat))
            {
                return stat;
            }
            else
            {
                _statList[key] = new Stat(key, 0);
                return _statList[key];
            }
        }

        public bool TryGetStat(StatKey key, out Stat stat)
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

            foreach (var stat in Stats)
            {
                list.Add(stat.SaveToJson());
            }
            
            return list;
        }

        internal void Reset()
        {
            foreach (var s in _statList.Values)
            {
                var stat = _statList[s.key];
                stat.Value = 0;
            }
        }
    }
}