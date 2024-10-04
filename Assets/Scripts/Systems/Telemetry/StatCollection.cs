using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.U2D.IK;

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
                TryGetStat(key, out var stat);
                return stat;
            }
        }
        
        public Stat GetStat(StatKey key)
        {
            if (_statList.TryGetValue(key, out var stat))
            {
                return stat;
            }
            return new Stat(key, 0);
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