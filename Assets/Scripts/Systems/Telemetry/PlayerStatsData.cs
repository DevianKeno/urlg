using System;
using System.Collections.Generic;

namespace RL.Telemetry
{
    [Serializable]
    public struct URLGSaveData
    {
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
        public List<DataEntry> Entries;
    }
}