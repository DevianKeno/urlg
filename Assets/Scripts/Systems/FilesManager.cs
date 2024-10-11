using System.IO;

using UnityEngine;
using Newtonsoft.Json;

using RL.Telemetry;
using UnityEditor;

namespace RL.Systems
{
    public class FilesManager : MonoBehaviour
    {
        public void SaveDataJson(URLGSaveData data)
        {
            var filename = "playerdata.json";
            var json = JsonConvert.SerializeObject(data);

            var savepath = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

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
            var path = Path.Combine(Application.persistentDataPath, "saves");
            
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