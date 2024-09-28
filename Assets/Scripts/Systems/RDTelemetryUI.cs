using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using URLG.CellularAutomata;

namespace RL
{
    [Serializable]
    public class TelemetryEntry
    {
        public string Id;
        int value;
        public int Value
        {
            get
            {
                value = int.Parse(inputField.text);
                return value;
            }
            set
            {
                this.value = value;
                inputField.text = this.value.ToString();
            }
        }
        public TMP_InputField inputField;
    }

    public class RDTelemetryUI : MonoBehaviour
    {
        public int FeatureCount = 20;

        public List<TelemetryEntry> entries = new();
        Dictionary<string, TelemetryEntry> entryIdMapping = new();

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
                if (!t.TryGetComponent<TMP_InputField>(out var inputField)) continue;
                
                var pairedTmp = statsContainer.transform.GetChild(t.GetSiblingIndex());
                var tmp = pairedTmp.GetComponent<TextMeshProUGUI>();
                var entry = new TelemetryEntry()
                {
                    Id = tmp.text,
                    inputField = inputField,
                };
                entries.Add(entry);
                entryIdMapping[tmp.text] = entry;
            }
        }

        public TelemetryEntry GetEntry(string id)
        {
            entryIdMapping.TryGetValue(id, out var entry);
            return entry;
        }

        public void OnRoomClick(MockRoom room)
        {
            if (room == null) return;
            if (room.Stats == null) return;

            var stats = room.Stats;
            int enemyTotal = stats.GetStat("EnemyCountFire").Value + stats.GetStat("EnemyCountBeam").Value + stats.GetStat("EnemyCountWave").Value;
            int obsTotal = stats.GetStat("ObstacleCountFire").Value + stats.GetStat("ObstacleCountBeam").Value + stats.GetStat("ObstacleCountWave").Value;

            featureDataTmp.text =  $@"<b>Enemies</b>
Fire weak: {stats.GetStat("EnemyCountFire").Value}
Beam weak: {stats.GetStat("EnemyCountBeam").Value}
Wave weak: {stats.GetStat("EnemyCountWave").Value}
Total: {enemyTotal}

<b>Obstacles</b>
Fire obstacles: {stats.GetStat("ObstacleCountFire").Value}
Beam obstacles: {stats.GetStat("ObstacleCountBeam").Value}
Wave obstacles: {stats.GetStat("ObstacleCountWave").Value}
Total: {obsTotal}
";
        }

        public void RandomizeUseCount()
        {
            (int, int, int) randUseCount = GenerateRandomTotaled(1000);
            GetEntry("Fire Use Count").Value = randUseCount.Item1;
            GetEntry("Beam Use Count").Value = randUseCount.Item2;
            GetEntry("Wave Use Count").Value = randUseCount.Item3;

            GetEntry("Fire Hit Count").Value = Rand(0, GetEntry("Fire Use Count").Value + 1);
            GetEntry("Beam Hit Count").Value = Rand(0, GetEntry("Beam Use Count").Value + 1);
            GetEntry("Wave Hit Count").Value = Rand(0, GetEntry("Wave Use Count").Value + 1);
        }

        public (int, int, int) GenerateRandomTotaled(int total)
        {
            int cut1 = UnityEngine.Random.Range(0, total + 1);
            int cut2 = UnityEngine.Random.Range(0, total + 1);
            int first = Mathf.Min(cut1, cut2);
            int second = Mathf.Max(cut1, cut2);

            return (first, second - first, total - second);
        }

        public static int Rand(int minIncl, int maxExcl)
        {
            return UnityEngine.Random.Range(minIncl, maxExcl);
        } 
    }
}