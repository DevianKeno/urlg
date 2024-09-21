using System.Collections.Generic;
using System.Linq;

namespace URLG.Telemetry
{
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
                if (_statList.ContainsKey(name))
                {
                    return _statList[name];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public bool TryGetStat(string name, out Stat stat)
        {
            if (_statList.ContainsKey(name))
            {
                stat = _statList[name];
                return true;
            }
            else
            {
                stat = default;
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
                var stat = _statList[s.Name];
                stat.Value = 0;
            }
        }
    }
}