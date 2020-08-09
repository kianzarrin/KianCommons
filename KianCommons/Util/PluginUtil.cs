
namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using ColossalFramework.Plugins;
    using ICities;
    using System.Reflection;

    public static class PluginUtil {
        internal static bool CSUREnabled;
        static bool IsCSUR(PluginManager.PluginInfo current) =>
            current.name.Contains("CSUR ToolBox") || 1959342332 == (uint)current.publishedFileID.AsUInt64;
        public static void Init() {
            CSUREnabled = false;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (!current.isEnabled) continue;
                if (IsCSUR(current)) {
                    CSUREnabled = true;
                    Log.Debug(current.name + "detected");
                    return;
                }
            }
        }

        public static PluginManager.PluginInfo GetPlugin(IUserMod userMod) {
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (userMod == current.userModInstance)
                    return current;
            }
            return null;
        }

        public static PluginManager.PluginInfo GetPlugin(Assembly assembly = null) {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (current.ContainsAssembly(assembly))
                    return current;
            }
            return null;
        }

        public static PluginManager.PluginInfo GetPlugin(string name, ulong workshopId) {
            string Simplify(string s) =>
                s.ToLower().Replace(" ", "");
            name = Simplify(name);
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                string name2 = Simplify(current.name);
                ulong workshopId2 = current.publishedFileID.AsUInt64;
                if (name2.StartsWith(name) || workshopId2 == workshopId)
                    return current;
            }
            return null;
        }
    }
}
