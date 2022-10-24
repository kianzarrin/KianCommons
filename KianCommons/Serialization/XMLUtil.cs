using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Linq;
using UnityEngine;
using System.Linq;
using ColossalFramework.Math;

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
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(writer)) {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.Namespaces = false;
                        Serialize(writer, value);
                        return writer.ToString();
                    }
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

        public static Version ExtractVersion(string xmlData) {
#if false
            // Regex maybe faster?
            var rx = new Regex(@"Version='([\.\d]+)'".Replace("'", "\""));
            var match = rx.Match(xmlData);
            string version = match.Groups[1].Value;
            return new Version(version);
#endif
            XDocument xdoc = XDocument.Parse(xmlData);
            string version = xdoc.Root.Attribute("version").Value;
            return new Version(version);
        }


        public static object XMLConvert(object value, Type type) {
            //if (value is Vector3 vector3 && type == typeof(Vector3Serializable))
            //    return (Vector3Serializable)vector3;
            //if (value is Vector3Serializable xmlVector3 && type == typeof(Vector3))
            //    return (Vector3)xmlVector3;

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

    public struct Vector3XML : IXmlSerializable {
        public Vector3 Vector;
        private float[] dims => new[] { Vector[0], Vector[1], Vector[2] };
        public static implicit operator Vector3(Vector3XML v) => v.Vector;
        public static implicit operator Vector3XML(Vector3 v) => new Vector3XML { Vector = v };
        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader) {
            int d = 0;
            foreach (string s in reader.ReadString().Split(',')) {
                Vector[d++] = float.Parse(s);
            }
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteString(ToString());
        }
        public override string ToString() {
            var strDims = dims.Select(v => v.ToString("G9"));
            return string.Join(", ", strDims.ToArray());
        }
    }

    public struct Bezier3XML {
        public Vector3XML A, B, C, D;
        public static implicit operator Bezier3(Bezier3XML v) => new Bezier3(v.A, v.B, v.C, v.D);
        public static implicit operator Bezier3XML(Bezier3 v) => new Bezier3XML { A = v.a, B = v.b, C = v.c, D = v.d };
    }
}
