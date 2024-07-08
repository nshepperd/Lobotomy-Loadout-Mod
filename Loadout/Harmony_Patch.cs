using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Harmony;

namespace Loadout {
    public class Harmony_Patch {
        public static string ModName = "Lobotomy.synthropy.Loadout";

        public static FileManager fileManager;
        public static LoadoutManager loadoutManager;

		public Harmony_Patch() {
            try {
                HarmonyInstance harmony = HarmonyInstance.Create(Harmony_Patch.ModName);
                fileManager = new Loadout.FileManager(ModName);
                loadoutManager = new Loadout.LoadoutManager(fileManager);
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
            } catch (Exception e) {
                Logging.Info("Exception: \n" + e.Message + "\n" + e.StackTrace);
            }
		}
    }
}
