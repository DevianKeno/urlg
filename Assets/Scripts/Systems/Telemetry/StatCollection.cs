using System;
using System.Collections.Generic;
using System.Linq;

namespace URLG.Telemetry
{
    [Serializable]
    public class StatCollection
    {
        Dictionary<string, Stat> _statList = new();
        public List<Stat> Stats => _statList.Values.ToList();

        public StatCollection(string[] stats)
        {
            foreach (var stat in stats)
            {
                _statList[stat] = new Stat(stat, 0);
            }
        }

        public Stat this[string name]
        {
            get
            {
                _statList.TryGetValue(name, out var stat);
                return stat;
            }
        }
        
        public Stat GetStat(string name)
        {
            if (_statList.TryGetValue(name, out var stat))
            {
                return stat;
            }
            return null;
        }

        public bool TryGetStat(string name, out Stat stat)
        {
            if (_statList.TryGetValue(name, out stat))
            {
                return true;
            }
            return false;
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
                var stat = _statList[s.Name];
                stat.Value = 0;
            }
        }
    }
}