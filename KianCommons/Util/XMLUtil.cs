using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace KianCommons {
    internal static class XMLSerializerUtil {
        static XmlSerializer Serilizer<T>() => new XmlSerializer(typeof(T));
        static void Serialize<T>(TextWriter writer, T value) => Serilizer<T>().Serialize(writer, value);
        static T Deserialize<T>(TextReader reader) => (T)Serilizer<T>().Deserialize(reader);

        public static string Serialize<T>(T value) {
            try {
                using (TextWriter writer = new StringWriter()) {
                    Serialize<T>(writer, value);
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
                Log.Exception(ex);
                return default;
            }
        }

        public static void WriteToFileWrapper(string fileName, string data, Version version = null) {
            try {
                version = version ?? typeof(XMLSerializerUtil).VersionOf();
                using (StreamWriter sw = File.CreateText(fileName)) {
                    sw.WriteLine("Version=" + version);
                    sw.WriteLine(data);
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static string ReadFromFileWrapper(string fileName, out Version version) {
            try {
                using (StreamReader sw = File.OpenText(fileName)) {
                    string lineVersion = sw.ReadLine();
                    int i = lineVersion.IndexOf("=");
                    string strVersion = lineVersion.Substring(i + 1);
                    version = new Version(strVersion);

                    return sw.ReadToEnd();
                }
            } catch (Exception ex) {
                Log.Exception(ex);
                version = default;
                return null;
            }
        }

        public static object XMLConvert(object value, Type type) {
            if (value is Vector3 vector3 && type == typeof(XmlVector3))
                return (XmlVector3)vector3;
            if (value is XmlVector3 xmlVector3 && type == typeof(Vector3))
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
    public class XmlVector3 : IXmlSerializable {
        [XmlIgnore] public Vector3 v_;

        public XmlSchema GetSchema() => null;
        public void WriteXml(XmlWriter writer) =>
            writer.WriteString($"x:{v_.x} y:{v_.y} z:{v_.z}");

        public void ReadXml(XmlReader reader) {
            string data = reader.ReadString();
            if (string.IsNullOrEmpty(data)) {
                v_ = default;
                return;
            }

            data = data.Remove("x:", "y:", "z:");
            var datas = data.Split(" ");

            v_.x = float.Parse(datas[0]);
            v_.y = float.Parse(datas[1]);
            v_.z = float.Parse(datas[2]);
        }

        public XmlVector3() { } // XML constructor.
        public XmlVector3(Vector3 v) => v_ = v;
        public static implicit operator XmlVector3(Vector3 v) => new XmlVector3(v);
        public static implicit operator Vector3(XmlVector3 xmlv) => xmlv.v_;
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
}
