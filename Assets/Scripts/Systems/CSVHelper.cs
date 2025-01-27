/*
Component Title: Audio Manager
Data written: October 5, 2024
Date revised: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This is a helper program to help write CSV files to a target path
    given an input of a List of strings.

Data Structures:
    N/A
*/

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

        /// UNUSED
        public static void WriteCSV()
        {

        }
    }
}