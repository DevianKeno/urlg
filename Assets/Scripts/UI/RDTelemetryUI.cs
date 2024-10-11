using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using RL.CellularAutomata;
using RL.Telemetry;
using System.IO;

namespace RL.UI
{
    public class RDTelemetryUI : MonoBehaviour
    {
        MockRoom room = null;
        public int FeatureCount = 20;
        public int Seed;
        bool _seedIsDirty = false;

        PlayerStatCollection currentPlayerStat;
        RoomStatCollection currentRoomStat;

        public List<TelemetryEntryUI> entries = new();
        Dictionary<StatKey, TelemetryEntryUI> entryIdMapping = new();

        [SerializeField] GameObject statsContainer;
        [SerializeField] GameObject fieldsContainer; 
        [SerializeField] TextMeshProUGUI featureDataTmp;

        void Start()
        {
            InitializeElements();
        }

        [ContextMenu("Initialize")]
        public void InitializeElements()
        {
            entries.Clear();
            entryIdMapping.Clear();

            foreach (Transform t in fieldsContainer.transform)
            {
                if (!t.TryGetComponent<TelemetryEntryUI>(out var entry)) continue;
                
                entries.Add(entry);
                entryIdMapping[entry.Key] = entry;
            }

            var seedField = GetEntry(StatKey.Seed);
            if (seedField != null)
            {
                var inputField = seedField.GetComponent<TMP_InputField>();
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener(SetSeedDirty);
            }
        }

        void SetSeedDirty(string arg0)
        {
            _seedIsDirty = true;
        }

        public TelemetryEntryUI GetEntry(StatKey key)
        {
            entryIdMapping.TryGetValue(key, out var entry);
            return entry;
        }

        public void OnRoomClick(MockRoom room)
        {
            if (room == null) return;
            if (room.Stats == null) return;

            var stats = room.Stats;
            int enemyTotal = stats.TotalEnemyCount;
            int obsTotal = stats.TotalObstacleCount;
            
            featureDataTmp.text =  $@"<b>Enemies</b>
Fire weak: {stats.GetStat(StatKey.EnemyCountFire).Value}
Beam weak: {stats.GetStat(StatKey.EnemyCountBeam).Value}
Wave weak: {stats.GetStat(StatKey.EnemyCountWave).Value}
Total: {enemyTotal}

<b>Obstacles</b>
Fire obstacles: {stats.GetStat(StatKey.ObstacleCountFire).Value}
Beam obstacles: {stats.GetStat(StatKey.ObstacleCountBeam).Value}
Wave obstacles: {stats.GetStat(StatKey.ObstacleCountWave).Value}
Total: {obsTotal}
";
        }


        #region Player telemetry randomizer
        public void RandomizeUseCount()
        {
            Seed = GetNewSeed();
            GetEntry(StatKey.Seed).Value = Seed;
            (int, int, int) randUseCount = GenerateRandomTotaled(1000, Seed);
            GetEntry(StatKey.UseCountFire).Value = randUseCount.Item1;
            GetEntry(StatKey.UseCountBeam).Value = randUseCount.Item2;
            GetEntry(StatKey.UseCountWave).Value = randUseCount.Item3;

            /// Hit count cannot be above Use count
            GetEntry(StatKey.HitCountFire).Value = Rand(0, GetEntry(StatKey.UseCountFire).Value + 1, Seed);
            GetEntry(StatKey.HitCountBeam).Value = Rand(0, GetEntry(StatKey.UseCountBeam).Value + 1, Seed);
            GetEntry(StatKey.HitCountWave).Value = Rand(0, GetEntry(StatKey.UseCountWave).Value + 1, Seed);
        }

        public PlayerStatCollection ConstructPlayerTelemetryStats()
        {
            var stats = new PlayerStatCollection(Telemetry.Telemetry.PlayerStatsKeys);

            stats[StatKey.UseCountFire].Value = GetEntry(StatKey.UseCountFire).Value;
            stats[StatKey.UseCountBeam].Value = GetEntry(StatKey.UseCountBeam).Value;
            stats[StatKey.UseCountWave].Value = GetEntry(StatKey.UseCountWave).Value;

            stats[StatKey.HitCountFire].Value = GetEntry(StatKey.HitCountFire).Value;
            stats[StatKey.HitCountBeam].Value = GetEntry(StatKey.HitCountBeam).Value;
            stats[StatKey.HitCountWave].Value = GetEntry(StatKey.HitCountWave).Value;

            stats[StatKey.HitsTaken].Value = GetEntry(StatKey.HitsTaken).Value;
            stats.Seed = Seed;

            return stats;
        }
        
        public static PlayerStatCollection ConstructPlayerRandom(int maxUseCount)
        {
            var stats = new PlayerStatCollection(Telemetry.Telemetry.PlayerStatsKeys);
            int seed = NewSeed();

            (int, int, int) randUseCount = GenerateRandomTotaled(maxUseCount, seed);
            stats[StatKey.UseCountFire].Value = randUseCount.Item1;
            stats[StatKey.UseCountBeam].Value = randUseCount.Item2;
            stats[StatKey.UseCountWave].Value = randUseCount.Item3;

            stats[StatKey.HitCountFire].Value = Rand(0, stats[StatKey.UseCountFire].Value + 1, seed);
            stats[StatKey.HitCountBeam].Value = Rand(0, stats[StatKey.UseCountBeam].Value + 1, seed);
            stats[StatKey.HitCountWave].Value = Rand(0, stats[StatKey.UseCountWave].Value + 1, seed);

            stats[StatKey.HitsTaken].Value = Rand(0, 100, seed);
            stats.Seed = seed;

            return stats;
        }
        
        #endregion


        #region Room telemetry randomizer
        public void RandomizeRoomFeatures()
        {
            Seed = GetNewSeed();
            GetEntry(StatKey.Seed).Value = Seed;
            (int, int, int) randEnemyCounts = GenerateRandomTotaled(GetEntry(StatKey.MaxEnemyCount).Value, Seed);
            GetEntry(StatKey.EnemyCountFire).Value = randEnemyCounts.Item1;
            GetEntry(StatKey.EnemyCountBeam).Value = randEnemyCounts.Item2;
            GetEntry(StatKey.EnemyCountWave).Value = randEnemyCounts.Item3;

            (int, int, int) randObsCounts = GenerateRandomTotaled(GetEntry(StatKey.MaxObstacleCount).Value, Seed);
            GetEntry(StatKey.ObstacleCountFire).Value = randObsCounts.Item1;
            GetEntry(StatKey.ObstacleCountBeam).Value = randObsCounts.Item2;
            GetEntry(StatKey.ObstacleCountWave).Value = randObsCounts.Item3;
        }

        public RoomStatCollection ConstructRoomTelemetryStats()
        {
            var stats = new RoomStatCollection(Telemetry.Telemetry.RoomStatsKeys);

            foreach (var key in Telemetry.Telemetry.RoomStatsKeys)
            {
                stats[key].Value = GetEntry(key).Value;
            }
            stats.Seed = Seed;
            
            return stats;
        }

        public static RoomStatCollection ConstructRoomRandom(int maxEnemyCount, int maxObstacleCount)
        {
            var stats = new RoomStatCollection(Telemetry.Telemetry.RoomStatsKeys);
            int seed = NewSeed();

            (int, int, int) randEnemyCounts = GenerateRandomTotaled(maxEnemyCount, seed);
            stats[StatKey.EnemyCountFire].Value = randEnemyCounts.Item1;
            stats[StatKey.EnemyCountBeam].Value = randEnemyCounts.Item2;
            stats[StatKey.EnemyCountWave].Value = randEnemyCounts.Item3;

            (int, int, int) randObsCounts = GenerateRandomTotaled(maxObstacleCount, seed);
            stats[StatKey.ObstacleCountFire].Value = randObsCounts.Item1;
            stats[StatKey.ObstacleCountBeam].Value = randObsCounts.Item2;
            stats[StatKey.ObstacleCountWave].Value = randObsCounts.Item3;
            stats.Seed = seed;
            
            return stats;
        }

        #endregion

        public void ResetSeed()
        {
            _seedIsDirty = false;
        }

        int GetNewSeed()
        {
            if (_seedIsDirty)
            {
                return GetEntry(StatKey.Seed).Value;
            }
            else
            {
                System.Random rand = new();
                return rand.Next(int.MinValue, int.MaxValue); 
            }
        }
        
        public static int NewSeed()
        {
            System.Random rand = new();
            return rand.Next(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Generates three random numbers split along a total.
        /// </summary>
        public static (int, int, int) GenerateRandomTotaled(int total, int seed)
        {
            UnityEngine.Random.InitState(seed);
            int cut1 = UnityEngine.Random.Range(0, total + 1);
            int cut2 = UnityEngine.Random.Range(0, total + 1);
            int first = Mathf.Min(cut1, cut2);
            int second = Mathf.Max(cut1, cut2);

            return (first, second - first, total - second);
        }

        public static int Rand(int minIncl, int maxExcl, int seed)
        {
            UnityEngine.Random.InitState(seed);
            return UnityEngine.Random.Range(minIncl, maxExcl);
        } 
    }
}