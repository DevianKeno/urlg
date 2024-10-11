using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using TMPro;

using RL.Telemetry;
using RL.UI;
using RL.Classifiers;

namespace RL.RD
{
    public struct ARDataEntry
    {
        public int GroundTruth { get; set; }
        public int SeedPlayer { get; set; }
        public int SeedRoom { get; set; }
        public Dictionary<StatKey, int> Values { get; set; }
    }

    public class Datagen : MonoBehaviour
    {
        [Header("Parameters")]
        public int EntryCount = 32767;
        public int RoomsPerPlayerPreference = 32;
        [Range(0, 100)] public float FluctuationChance = 33;
        public float AcceptanceThreshold = 0.2f;

        [Header("Feature Parameters")]
        public int MaxUseCount = 1000;
        public int MaxEnemyCount = 15;
        public int MaxObstacleCount = 6;

        float progress = 0f;

        [SerializeField] RDTelemetryUI playerTelemetry;
        [SerializeField] RDTelemetryUI roomTelemetry;
        [SerializeField] TextMeshProUGUI statusTmp;

        void FixedUpdate()
        {
            if (gameObject.activeInHierarchy)
            {
                statusTmp.text = $"Generating dataset... {progress * 100}%";
            }
        }

        [ContextMenu("Generate Dataset")]
        public void GenerateDataset()
        {
            StartCoroutine(GenerateDatasetEntries());
        }

        IEnumerator GenerateDatasetEntries()
        {
            statusTmp.text = $"Generating dataset... {progress}%";
            float startTime = Time.time;

            var entries = new List<ARDataEntry>();
            PlayerStatCollection playerStats = RDTelemetryUI.ConstructPlayerRandom(MaxUseCount);

            int roomsLeft = RoomsPerPlayerPreference;
            for (int i = EntryCount; i > 0; i--)
            {
                /// Refresh/get a new player preference
                if (roomsLeft <= 0)
                {
                    playerStats = RDTelemetryUI.ConstructPlayerRandom(MaxUseCount);
                    roomsLeft = RoomsPerPlayerPreference;
                }

                /// Generate accepted rooms for current player preference
                for (int acceptLeft = RoomsPerPlayerPreference / 2; acceptLeft > 0; acceptLeft--)
                {
                    var acceptedRoom = GenerateTargetRoom(playerStats, Status.Accepted);
                    entries.Add(CreateDataEntry(playerStats, acceptedRoom, Status.Accepted));
                    roomsLeft--;
                    i--;
                }

                /// Generate rejected rooms for current player preference
                for (int rejectLeft = RoomsPerPlayerPreference / 2; rejectLeft > 0; rejectLeft--)
                {
                    var rejectedRoom = GenerateTargetRoom(playerStats, Status.Rejected);
                    entries.Add(CreateDataEntry(playerStats, rejectedRoom, Status.Rejected));
                    roomsLeft--;
                    i--;
                }

                progress = i / RoomsPerPlayerPreference;
            }
            statusTmp.text= $"Done generation.";

            statusTmp.text= $"Writing to file...";
            WriteToCSV(entries);

            float elapsedTime = Time.time - startTime;
            statusTmp.text= $"Dataset generated in {elapsedTime:##} ms";

            yield return null;
        }

        ARDataEntry CreateDataEntry(PlayerStatCollection playerStats, RoomStatCollection roomStats, Status status)
        {
            var entry = new ARDataEntry
            {
                SeedPlayer = playerStats.Seed,
                SeedRoom = roomStats.Seed,
                Values = new()
            };

            foreach (var key in Telemetry.Telemetry.PlayerStatsKeys)
            {
                entry.Values[key] = playerStats.GetStat(key).Value;
            }

            foreach (var key in Telemetry.Telemetry.RoomStatsKeys)
            {
                entry.Values[key] = roomStats.GetStat(key).Value;
            }

            entry.GroundTruth = GetGroundTruth(status);
            return entry;
        }

        int GetGroundTruth(Status status)
        {
            if (status == Status.Accepted)
            {
                if (UnityEngine.Random.Range(0, 101) <= FluctuationChance)
                    return 0;
                else
                    return 1;
            }
            else if (status == Status.Rejected)
            {
                if (UnityEngine.Random.Range(0, 101) <= FluctuationChance)
                    return 1;
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }

        RoomStatCollection GenerateTargetRoom(PlayerStatCollection playerStats, Status targetStatus)
        {
            for (int i = 0; i < AcceptRejectRD.MaxBulkGenerationTimes; i++)
            {
                var roomStats = RDTelemetryUI.ConstructRoomRandom(MaxEnemyCount, MaxObstacleCount);

                ARResult result = ARClassifier.Classify(playerStats, roomStats, AcceptanceThreshold, normalized: true);

                if (result.Status == targetStatus)
                {
                    return roomStats;
                }
            }

            return new(Telemetry.Telemetry.RoomStatsKeys);
        }

        public void WriteToCSV(List<ARDataEntry> entries)
        {
            string directory = Path.Combine(Application.persistentDataPath, "dataset");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            string filepath = Path.Combine(directory, $"dataset-{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            using (StreamWriter writer = new StreamWriter(filepath))
            {
                var playerStatKeys = Telemetry.Telemetry.PlayerStatsKeys.Select(key => key.ToString()).ToArray();
                var roomStatKeys = Telemetry.Telemetry.RoomStatsKeys.Select(key => key.ToString()).ToArray();
                var headers = new[] { "SeedPlayer", "SeedRoom" }
                    .Concat(playerStatKeys)
                    .Concat(roomStatKeys)
                    .Append("GroundTruth");

                writer.WriteLine(string.Join(",", headers));

                foreach (var entry in entries)
                {
                    var row = new List<string>
                    {
                        entry.SeedPlayer.ToString(),
                        entry.SeedRoom.ToString()
                    };

                    foreach (var key in playerStatKeys)
                    {
                        row.Add(entry.Values.ContainsKey((StatKey) Enum.Parse(typeof(StatKey), key))
                            ? entry.Values[(StatKey) Enum.Parse(typeof(StatKey), key)].ToString()
                            : "0");
                    }

                    foreach (var key in roomStatKeys)
                    {
                        row.Add(entry.Values.ContainsKey((StatKey) Enum.Parse(typeof(StatKey), key))
                            ? entry.Values[(StatKey) Enum.Parse(typeof(StatKey), key)].ToString()
                            : "0");
                    }
                    
                    row.Add(entry.GroundTruth.ToString());

                    writer.WriteLine(string.Join(",", row));
                }
            }

            Debug.Log($"Dataset generated at '{filepath}'");
        }
    }
}
