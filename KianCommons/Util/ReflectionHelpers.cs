namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.Assertion;

    internal static class ReflectionHelpers {
        internal static Version Version(this Assembly asm) =>
          asm.GetName().Version;

        internal static Version VersionOf(this Type t) =>
            t.Assembly.GetName().Version;

        internal static Version VersionOf(this object obj) =>
            VersionOf(obj.GetType());

        internal static string Name(this Assembly assembly) => assembly.GetName().Name;

        internal static void CopyProperties(object target, object origin) {
            var t1 = target.GetType();
            var t2 = origin.GetType();
            Assert(t1 == t2 || t1.IsSubclassOf(t2));
            FieldInfo[] fields = origin.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void CopyProperties<T>(object target, object origin) {
            Assert(target is T, "target is T");
            Assert(origin is T, "origin is T");
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static string GetPrettyFunctionName(MethodInfo m) {
            string s = m.Name;
            string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 3)
                return ss[1];
            return s;
        }

        internal static bool HasAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute {
            var att = member.GetCustomAttributes(typeof(T), inherit);
            return att != null && att.Length != 0;
        }

        internal static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>(
            this object obj, bool inherit = true) where T : Attribute {
            return obj.GetType().GetFields()
                .Where(_field => _field.HasAttribute<T>(inherit));
        }

        public const BindingFlags ALL = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        /// <summary>
        /// get value of the instant field target.Field.
        /// </summary>
        internal static object GetFieldValue(string fieldName, object target) {
            var type = target.GetType();
            var field = type.GetField(fieldName, ALL)
                ?? throw new Exception($"{type}.{fieldName} not found");
            return field.GetValue(target);
        }

        /// <summary>
        /// Get value of a static field from T.fieldName
        /// </summary>
        internal static object GetFieldValue<T>(string fieldName)
            => GetFieldValue(fieldName, typeof(T));

        /// <summary>
        /// Get value of a static field from type.fieldName
        /// </summary>
        internal static object GetFieldValue(string fieldName, Type type) {
            var field = type.GetField(fieldName, ALL)
                ?? throw new Exception($"{type}.{fieldName} not found");
            return field.GetValue(null);
        }

    }
}
