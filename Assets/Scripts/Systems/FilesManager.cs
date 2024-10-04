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

        public void OpenSaveFolder()
        {
            var savepath = Path.Combine(Application.persistentDataPath, "saves");
            
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            
#if UNITY_STANDALONE_WIN /// Windows
                System.Diagnostics.Process.Start("explorer.exe", savepath.Replace('/', '\\'));
#elif UNITY_STANDALONE_OSX /// macOS
                System.Diagnostics.Process.Start("open", folderPath);
#endif
        }
    }
}