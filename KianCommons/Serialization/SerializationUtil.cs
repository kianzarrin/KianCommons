namespace KianCommons.Serialization {
    using UnityEngine;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;

    internal static class SerializationUtil {
        public static Version DeserializationVersion;

        static BinaryFormatter GetBinaryFormatter =>
            new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };

        public static object Deserialize(byte[] data, Version version) {
            if (data == null || data.Length==0) return null;
            try {
                DeserializationVersion = version;
                //Log.Debug($"SerializationUtil.Deserialize(data): data.Length={data?.Length}");
                var memoryStream = new MemoryStream();
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Position = 0;
                return GetBinaryFormatter.Deserialize(memoryStream);
            }
            catch (Exception e) {
                Log.Exception(e,showInPanel:false);
                return null;
            } finally {
                DeserializationVersion = null;
            }
        }

        public static byte[] Serialize(object obj) {
            if (obj == null) return null;
            var memoryStream = new MemoryStream();
            GetBinaryFormatter.Serialize(memoryStream, obj);
            memoryStream.Position = 0; // redundant
            return memoryStream.ToArray();
        }

        public static void GetObjectFields(SerializationInfo info, object instance) {
            var fields = instance.GetType().GetFields(ReflectionHelpers.COPYABLE).Where(field => !field.HasAttribute<NonSerializedAttribute>());
            foreach (FieldInfo field in fields) {
                var type = field.GetType();
                if (type == typeof(Vector3)) {
                    //Vector3Serializable v = (Vector3Serializable)field.GetValue(instance);
                    info.AddValue(field.Name, field.GetValue(instance), typeof(Vector3Serializable));
                } else { 
                    info.AddValue(field.Name, field.GetValue(instance), field.FieldType);
                }
            }
        }

        public static void SetObjectFields<TClass>(SerializationInfo info, TClass instance) where TClass : class {
            foreach (SerializationEntry item in info) {
                FieldInfo field = instance.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                if (field != null) {
                    object val = Convert.ChangeType(item.Value, field.FieldType);
                    field.SetValue(instance, val);
                }
            }
        }

        public static void SetObjectProperties<TClass>(SerializationInfo info, TClass instance) where TClass: class {
            foreach (SerializationEntry item in info) {
                var p = instance.GetType().GetProperty(item.Name, ReflectionHelpers.COPYABLE);
                var setter = p?.GetSetMethod();
                if (setter != null) {
                    object val = Convert.ChangeType(item.Value, p.PropertyType);
                    p.SetValue(instance, val, null);
                }
            }
        }

        public static void SetObjectFields<TStruct>(SerializationInfo info, ref TStruct s) where TStruct : struct {
            foreach (SerializationEntry item in info) {
                FieldInfo field = s.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                if (field != null) {
                    object val = Convert.ChangeType(item.Value, field.FieldType);
                    field.SetValue(s, val);
                }
            }
        }

        public static void SetObjectProperties<TStruct>(SerializationInfo info, ref TStruct s) where TStruct : struct {
            foreach (SerializationEntry item in info) {
                var p = s.GetType().GetProperty(item.Name, ReflectionHelpers.COPYABLE);
                var setter = p?.GetSetMethod();
                if (setter != null) {
                    object val = Convert.ChangeType(item.Value, p.PropertyType);
                    p.SetValue(s, val, null);
                }
            }
        }

        public static T GetValue<T>(this SerializationInfo info, string name) =>
            (T)info.GetValue(name, typeof(T));
    }

    internal static class IOExtensions {
        public static void Write(this Stream stream, byte[] data) {
            stream.Write(data, 0, data.Length);
        }

        public static int Write(this Stream stream, Version version) {
            stream.Write(Version2Bytes(version), 0, VERSION_SIZE);
            return VERSION_SIZE;
        }
        public const int VERSION_SIZE = sizeof(int) * 4;

        public static byte[] Version2Bytes(Version version) {
            var ret = BitConverter.GetBytes(version.Major)
                .Concat(BitConverter.GetBytes(version.Minor))
                .Concat(BitConverter.GetBytes(version.Build))
                .Concat(BitConverter.GetBytes(version.Revision))
                .ToArray();
            Assertion.AssertEqual(ret.Length, VERSION_SIZE);
            return ret;
        }

        public static Version ReadVersion(this Stream stream) {
            var data = new byte[VERSION_SIZE];
            stream.Read(data, 0, VERSION_SIZE);
            return new Version(
                major: BitConverter.ToInt32(data, 0),
                minor: BitConverter.ToInt32(data, 0),
                build: BitConverter.ToInt32(data, 0),
                revision: BitConverter.ToInt32(data, 0));
        }

        public static byte[] ReadToEnd(this Stream s) {
            int n = (int)(s.Length - s.Position);
            var data = new byte[n];
            int n2 = s.Read(data, 0, n);
            Assertion.AssertEqual(n, n2);
            return data;
        }


    }
}
