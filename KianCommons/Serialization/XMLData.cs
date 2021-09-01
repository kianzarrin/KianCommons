/* from Keallu's Hide It mod. */

namespace KianCommons.Serialization {
    using ColossalFramework.IO;
    using System;
    using System.IO;
    using System.Linq;
    using ColossalFramework;

    public abstract class XMLData<C> where C : class, new() {
        private static C instance_;
        public static C Instance => instance_ ??= Load();

        public static C Load() {
            string configPath = GetPath();
            Log.Called("path:" + configPath);
            return XMLSerializerUtil.Deserialize<C>(configPath);
        }

        public void Save() {
            string configPath = GetPath(); 
            Log.Called("path:" + configPath);
            XMLSerializerUtil.Serialize(this as C);
        }

        private static string GetPath() => Path.Combine(DataLocation.executableDirectory, GetConfigPath());
        private static string GetConfigPath() {
            if (typeof(C).GetCustomAttributes(typeof(ConfigurationPathAttribute), true)
                .FirstOrDefault() is ConfigurationPathAttribute configPathAttribute) {
                return configPathAttribute.Value;
            } else {
                return typeof(C).Name + ".xml";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationPathAttribute : Attribute {
        public ConfigurationPathAttribute(string value) => Value = value;
        public string Value { get; private set; }
    }

}

