using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Harmony;

namespace Loadout2 {
    public class Harmony_Patch {
        public static string ModName = "Lobotomy.nshepperd.Loadout";

		public Harmony_Patch() {
			HarmonyInstance harmony = HarmonyInstance.Create(Harmony_Patch.ModName);
            FileLog.Log("did my code run?");
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
		}
    }
}
