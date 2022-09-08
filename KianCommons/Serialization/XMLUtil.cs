using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Linq;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace KianCommons.Serialization {
    internal static class XMLSerializerUtil {
        static XmlSerializer Serilizer<T>() => new XmlSerializer(typeof(T));
        static XmlSerializerNamespaces NoNamespaces {
            get {
                var ret = new XmlSerializerNamespaces();
                ret.Add("", "");
                return ret;
            }
        }

        static void Serialize<T>(TextWriter writer, T value) => Serilizer<T>().Serialize(writer, value, NoNamespaces);
        static T Deserialize<T>(TextReader reader) => (T)Serilizer<T>().Deserialize(reader);


        public static string Serialize<T>(T value) {
            try {
                using (TextWriter writer = new StringWriter()) {
                    Serialize(writer, value);
                    return writer.ToString();
                }
            } catch (Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        public static T Deserialize<T>(string data) {
            try {
                using (TextReader reader = new StringReader(data)) {
                    return Deserialize<T>(reader);
                }
            } catch (Exception ex) {
                Log.Debug("data=" + data);
                Log.Exception(ex, showInPanel: false);
                return default;
            }
        }

        [Obsolete("incomplete", true)]
        public static void ReadXml(this IXmlSerializable target, XmlReader reader) {
            Console.WriteLine("ReadXml()");
            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!empty) {
                while (reader.IsStartElement()) {
                    try {
                        string name = reader.Name;
                        string value = reader.ReadElementString();
                        Type type = target.GetType();
                        var field = type.GetField(name, BindingFlags.Instance);
                        if (field != null) {
                            field.SetValue(target, value);
                            continue;
                        }
                        var property = type.GetProperty(name, BindingFlags.Instance);
                        if (property?.GetSetMethod() != null) {
                            property.SetValue(target, value, null);
                        }
                    } catch(Exception ex) {
                        Log.Warning(ex.Message);
                    }
                }
                reader.ReadEndElement();
            }
        }

        [Obsolete("incomplete", true)]
        public static void WriteXml(this IXmlSerializable target, XmlWriter writer) {
            Type type = target.GetType();
            foreach (var field in type.GetFields(BindingFlags.Instance)) {
                if (!field.HasAttribute<XmlIgnoreAttribute>()) {
                    string value = field.GetValue(target)?.ToString();
                    if (value != null) {
                        writer.WriteElementString(field.Name, value);
                    }
                }
            }
            foreach (var property in type.GetProperties(BindingFlags.Instance)) {
                if (!property.HasAttribute<XmlIgnoreAttribute>()) {
                    string value = property.GetValue(target, null)?.ToString();
                    if (value != null) {
                        writer.WriteElementString(property.Name, value);
                    }
                }
            }
        }

        public static Version ExtractVersion(string xmlData) {
            if (false) {
                var rx = new Regex(@"Version='([\.\d]+)'".Replace("'", "\""));
                var match = rx.Match(xmlData);
                string version = match.Groups[1].Value;
                return new Version(version);
            } else {
                var document = new XmlDocument();
                document.LoadXml(xmlData);
                string version = document.DocumentElement.Attributes["Version"].Value;
                return new Version(version);
            }
        }


        public static void WriteToFile(string fileName, string data) {
            try {
                using (StreamWriter sw = File.CreateText(fileName)) {
                    sw.WriteLine(data);
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static string ReadFromFile(string fileName) {
            try {
                using (StreamReader sw = File.OpenText(fileName)) {
                    return sw.ReadToEnd();
                }
            } catch (Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        public static object XMLConvert(object value, Type type) {
            if (value is Vector3 vector3 && type == typeof(Vector3Serializable))
                return (Vector3Serializable)vector3;
            if (value is Vector3Serializable xmlVector3 && type == typeof(Vector3))
                return (Vector3)xmlVector3;

            object ret;
            ret = XMLPrefabConvert<PropInfo>(value, type);
            if (ret != null) return ret;
            ret = XMLPrefabConvert<TreeInfo>(value, type);
            if (ret != null) return ret;

            return null;
        }

        public static object XMLPrefabConvert<T>(object value, Type type) where T : PrefabInfo {
            if (value is T propInfo && type == typeof(XmlPrefabInfo<T>))
                return (XmlPrefabInfo<T>)propInfo;
            if (value is XmlPrefabInfo<T> xmlPropInfo && type == typeof(T))
                return (T)xmlPropInfo;
            return null;
        }
    }
    public class XmlPrefabInfo<T> : IXmlSerializable
        where T : PrefabInfo {
        public string name;
        public XmlPrefabInfo() { } // XML constructor.

        public XmlPrefabInfo(T prefab) => name = prefab.name;

        public static implicit operator XmlPrefabInfo<T>(T prefab) =>
            new XmlPrefabInfo<T>(prefab);

        public static implicit operator T(XmlPrefabInfo<T> prefab) {
            if (string.IsNullOrEmpty(prefab.name))
                return null;
            else
                return PrefabCollection<T>.FindLoaded(prefab.name);
        }

        public XmlSchema GetSchema() => null;
        public void WriteXml(XmlWriter writer) => writer.WriteString(name);
        public void ReadXml(XmlReader reader) => name = reader.ReadString();
    }

    public class XMLVersion : IXmlSerializable {
        Version version_;
        public static implicit operator Version(XMLVersion v) => v.version_;
        public static implicit operator XMLVersion(Version v) => new XMLVersion { version_ = v };

        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader) => version_ = new Version(reader.ReadString());
        public void WriteXml(XmlWriter writer) => writer.WriteString(version_.ToString());
    }
}
