using System;
using System.Collections.Generic;
using System.Linq;

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