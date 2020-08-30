namespace KianCommons {
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Reflection;

    public static class HarmonyUtil {
        public static string AssemblyName =>  Assembly.GetExecutingAssembly().GetName().Name;

        public static void AssertHarmonyInstalled() {
            if (!HarmonyHelper.IsHarmonyInstalled) {
                string m =
                    "****** ERRRROOORRRRRR!!!!!!!!!! **************\n" +
                    "**********************************************\n" +
                    "    HARMONY MOD DEPENDANCY IS NOT INSTALLED!\n\n" +
                    "solution: exit to desktop. unsub and resub to harmony mod. then run the game again.\n" +
                    "**********************************************\n" +
                    "**********************************************\n";
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        public static void InstallHarmony(string harmonyID) {
            AssertHarmonyInstalled();
            Log.Info("Patching...");
            var harmony = new Harmony(harmonyID);
            harmony.PatchAll();
            Log.Info("Patched.");
        }

        public static void UninstallHarmony(string harmonyID) {
            AssertHarmonyInstalled();
            Log.Info("UnPatching...");
            var harmony = new Harmony(harmonyID);
            harmony.UnpatchAll(harmonyID);
            Log.Info("UnPatched.");
        }
    }
}