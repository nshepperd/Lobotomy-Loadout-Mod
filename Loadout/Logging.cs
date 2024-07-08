using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Harmony;

namespace Loadout {
    public class Logging {
        #if DEBUG
        public static bool debugging = true;
        #else
        public static bool debugging = false;
        #endif
        
        public static void Debug(string message) {
            if (!debugging) return;
            FileLog.Log(message);
        }
        public static void Info(string message) {
            FileLog.Log(message);
        }
    }
}