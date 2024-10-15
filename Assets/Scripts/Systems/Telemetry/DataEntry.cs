using System;

namespace RL.Telemetry
{
    [Serializable]
    public struct DataEntry
    {
        public int LevelNumber { get; set; }
        public int Classification { get; set; }
        public int GroundTruth { get; set; }
        public PlayerStatCollection PlayerStats { get; set; }
        public RoomStatCollection RoomStats { get; set; }
    }
}