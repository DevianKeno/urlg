using RL.Classifiers;
using RL.Levels;
using RL.Telemetry;

namespace RL.Generator
{
    public struct FeaturizeOptions
    {
        public Room Room { get; set; }
        public Status TargetStatus { get; set; }
        public PCGAlgorithm Algorithm { get; set; }
        public PlayerStatCollection PlayerStats { get; set; }
        public int MaxEnemyCount { get; set; }
        public int MaxObstacleCount { get; set; }
        public RoomStatCollection RoomStats { get; set; }
    }
}