namespace RL.CellularAutomata
{
    /// <summary>
    /// Used to store settings (e.g., amount of enemies and obstacle to generate) for featurizing rooms.
    /// </summary>
    public struct FeatureParametersSettings
    {
        public int MaxEnemyCount { get; set; }
        public int MaxObstacleCount { get; set; }
    }
}