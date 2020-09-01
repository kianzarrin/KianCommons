namespace KianCommons {
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Reflection;
    using System;
    using System.Runtime.CompilerServices;

    public static class HarmonyUtil {
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
                throw new Exception(m);
            }
        }

        public static void InstallHarmony(string harmonyID) {
            AssertHarmonyInstalled();
            Log.Info("Patching...");
            PatchAll(harmonyID);
            Log.Info("Patched.");
        }

        /// <summary>
        /// assertion shall take place in a function that does not refrence Harmony.
        /// </summary>
        /// <param name="harmonyID"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.PatchAll();
        }

        public static void UninstallHarmony(string harmonyID) {
            AssertHarmonyInstalled();
            Log.Info("UnPatching...");
            UnpatchAll(harmonyID);
            Log.Info("UnPatched.");
        }

        static void UnpatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.UnpatchAll(harmonyID);
        }
    }
}