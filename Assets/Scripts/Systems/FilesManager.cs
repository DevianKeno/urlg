using System.IO;

using UnityEngine;
using Newtonsoft.Json;

using RL.Telemetry;
using UnityEditor;
using System;

namespace RL.Systems
{
    public class FilesManager : MonoBehaviour
    {
        const string SAVE_DATA_EXTENSION = "json";

        public void SaveDataJson(ResultsJsonData data)
        {
            var date = $"{DateTime.Now:yyyyMMdd_HHmmss}";
            
            string subfolder = "";
            if (Game.Main.AlgorithmUsed == PCGAlgorithm.AcceptReject)
                subfolder = "ar";
            else if (Game.Main.AlgorithmUsed == PCGAlgorithm.GaussianNaiveBayes)
                subfolder = "gnb";

            var filename = subfolder + "_results_" + date + $".{SAVE_DATA_EXTENSION}";
            
            var savepath = Path.Combine(Application.persistentDataPath, "results", subfolder);
            if (!Directory.Exists(savepath)) Directory.CreateDirectory(savepath);
            
            var json = JsonConvert.SerializeObject(data);

            var filepath = Path.Combine(savepath, filename);
            File.WriteAllText(filepath, json);
            Debug.Log($"Saved Telemetry data to {filepath}");
        }

        public void OpenDatasFolder()
        {
            var path = Path.Combine(Application.persistentDataPath, "dataset");
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
#if UNITY_STANDALONE_WIN /// Windows
                System.Diagnostics.Process.Start("explorer.exe", path.Replace('/', '\\'));
#elif UNITY_STANDALONE_OSX /// macOS
                System.Diagnostics.Process.Start("open", folderPath);
#endif
        }

        public void OpenSavesFolder()
        {
            var path = Path.Combine(Application.persistentDataPath, "results");
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
#if UNITY_STANDALONE_WIN /// Windows
                System.Diagnostics.Process.Start("explorer.exe", path.Replace('/', '\\'));
#elif UNITY_STANDALONE_OSX /// macOS
                System.Diagnostics.Process.Start("open", folderPath);
#endif
        }
    }
}