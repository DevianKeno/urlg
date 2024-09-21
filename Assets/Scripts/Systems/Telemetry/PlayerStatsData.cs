using System;
using System.Collections.Generic;

namespace URLG.Telemetry
{
    [Serializable]
    public struct URLGSaveData
    {
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
        public List<StatSaveData> PlayerStats;
        public List<StatSaveData> GameStats;
        public List<StatSaveData> RoomStats;
    }
}