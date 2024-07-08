using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Harmony;
using UnityEngine;

namespace Loadout {
    public class FileManager {
        private readonly string _dataPath;

        public FileManager(string modName){
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/BaseMods/" + modName);

            if (!directory.Exists) {
                FileLog.Log("Could not find mod directory for " + modName);
                return;
            }

            FileLog.Log("Using mod directory " + directory.FullName);
            _dataPath = directory.FullName;
        }

        public string ReadFile(string fileName) {
            if (_dataPath is null) {
                return null;
            }

            string filePath = Path.Combine(_dataPath, fileName);
            if (!File.Exists(filePath)) {
                return null;
            }

            return File.ReadAllText(filePath);
        }

        public void WriteFile(string fileName, string content) {
            if (_dataPath is null) {
                return;
            }

            string filePath = Path.Combine(_dataPath, fileName);
            File.WriteAllText(filePath, content);
        }
    }
}
