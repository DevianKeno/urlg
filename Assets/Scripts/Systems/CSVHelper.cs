using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace RL
{
    public class CSVHelper
    {
        public static List<string[]> ReadCSV(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Debug.LogError($"File not found '{filepath}'");
                return new();
            }

            var lines = File.ReadAllLines(filepath);
            string[] legend = lines[0].Split(',');
            var data = new List<string[]>();
            foreach (string line in lines)
            {
                data.Add(line.Split(','));
            }

            return data;
        }

        public static void WriteCSV()
        {

        }
    }
}