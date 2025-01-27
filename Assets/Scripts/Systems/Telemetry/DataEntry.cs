using System;
using System.Collections.Generic;

namespace RL.Telemetry
{
    [Serializable]
    public struct DataEntry
    {
        public int LevelNumber { get; set; }
        public int Classification { get; set; }
        public int GroundTruth { get; set; }
        public int DeathCount { get; set; }
        public List<StatSaveData> PlayerStats { get; set; }
        public List<StatSaveData> RoomStats { get; set; }
    }
}