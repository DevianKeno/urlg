using System;
using System.Collections.Generic;

namespace RL.Telemetry
{
    [Serializable]
    public struct ResultsJsonData
    {
        public string Algorithm;
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
        public List<DataEntry> Entries;
    }
}