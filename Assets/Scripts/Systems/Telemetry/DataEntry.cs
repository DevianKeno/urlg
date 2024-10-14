using System;

namespace RL.Telemetry
{
    [Serializable]
    public struct DataEntry
    {
        public int LevelNumber { get; set; }
        public int GroundTruth { get; set; }
        public PlayerStatCollection PlayerStats { get; set; }
        public RoomStatCollection RoomStats { get; set; }
        
        public DataEntry(int levelNumber, int groundTruth, PlayerStatCollection playerStats, RoomStatCollection roomStats)
        {
            LevelNumber = levelNumber;
            GroundTruth = groundTruth;
            PlayerStats = playerStats;
            RoomStats = roomStats;
        }
    }
}