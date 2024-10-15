using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

namespace RL.RD
{
    public class PerformanceMeasureRD : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] TextMeshProUGUI arFilenameTmp;
        [SerializeField] TextMeshProUGUI gnbFilenameTmp;

        [Header("Buttons")]
        [SerializeField] Button arResultsBtn;
        [SerializeField] Button gnbResultsBtn;

        void Awake()
        {
            arResultsBtn.onClick.AddListener(OpenARResults);
            gnbResultsBtn.onClick.AddListener(OpenGNBResults);
        }

        void OpenARResults()
        {
            var directory = Path.Combine(Application.persistentDataPath, "results", "ar");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select AR results file (.json)", directory, "json", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                arFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }

        void OpenGNBResults()
        {
            var directory = Path.Combine(Application.persistentDataPath, "results", "gnb");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select AR results file (.json)", directory, "json", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                gnbFilenameTmp.text = $"{Path.GetFileName(paths[0])}";
            }
        }
    }
}