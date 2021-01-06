namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.Assertion;
    using System.Diagnostics;
    using ColossalFramework;
    using ColossalFramework.UI;

    internal static class ReflectionHelpers {
        internal static Version Version(this Assembly asm) =>
          asm.GetName().Version;

        internal static Version VersionOf(this Type t) =>
            t.Assembly.GetName().Version;

        internal static Version VersionOf(this object obj) =>
            VersionOf(obj.GetType());

        internal static string Name(this Assembly assembly) => assembly.GetName().Name;

        public static string FullName(this MethodBase m) =>
            m.DeclaringType.FullName + "." + m.Name;


        internal static T ShalowClone<T>(this T source) where T : class, new() {
            T target = new T();
            CopyProperties<T>(target, source);
            return target;
        }

        internal static void CopyProperties(object target, object origin) {
            var t1 = target.GetType();
            var t2 = origin.GetType();
            Assert(t1 == t2 || t1.IsSubclassOf(t2));
            FieldInfo[] fields = origin.GetType().GetFields(ALL);
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
            FieldInfo[] fields = typeof(T).GetFields(ALL);
            foreach (FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        /// <summary>
        /// copies fields with identical name from origin to target.
        /// even if the declaring types don't match.
        /// only copies existing fields and their types match.
        /// </summary>
        internal static void CopyPropertiesForced<T>(object target, object origin) {
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                string fieldName = fieldInfo.Name;
                var originFieldInfo = origin.GetType().GetField(fieldName, ALL);
                var targetFieldInfo = target.GetType().GetField(fieldName, ALL);
                if(originFieldInfo !=null && targetFieldInfo != null) {
                    try {
                        object value = originFieldInfo.GetValue(origin);
                        targetFieldInfo.SetValue(target, value);
                    } catch { }
                }
            }
        }

        internal static void SetAllDeclaredFieldsToNull(object instance) {
            var type = instance.GetType();
            var fields = type.GetAllFields(declaredOnly:true);
            foreach(var f in fields) {
                if (f.FieldType.IsClass) {
                    if (HelpersExtensions.VERBOSE)
                        Log.Debug($"SetAllDeclaredFieldsToNull: setting {instance}.{f} = null");
                    f.SetValue(instance, null);
                }
            }
        }

        /// <summary>
        /// call this in OnDestroy() to clear all refrences.
        /// </summary>
        internal static void SetAllDeclaredFieldsToNull(this UIComponent c) =>
            SetAllDeclaredFieldsToNull(c as object);

        internal static string GetPrettyFunctionName(MethodInfo m) {
            string s = m.Name;
            string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 3)
                return ss[1];
            return s;
        }

        internal static T GetAttribute<T>(this MemberInfo member, bool inherit = true) where T:Attribute {
            return member.GetAttributes<T>().FirstOrDefault();
        }

        internal static T[] GetAttributes<T>(this MemberInfo member, bool inherit = true) where T : Attribute {
            return member.GetCustomAttributes(typeof(T), inherit) as T[];
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

        internal static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>(
            this Type type, bool inherit = true) where T : Attribute {
            return type.GetFields()
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
        /// sets target.fieldName to value.
        /// this works even if target is of struct type
        /// Post condtion: target has the new value
        /// </summary>
        internal static void SetFieldValue(string fieldName, object target, object value) {
            var type = target.GetType();
            var field = type.GetField(fieldName, ALL)
                ?? throw new Exception($"{type}.{fieldName} not found");
            field.SetValue(target, value);
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

        /// <summary>
        /// gets method of any access type.
        /// </summary>
        internal static MethodInfo GetMethod(Type type, string method, bool throwOnError=true) {
            if (type == null) throw new ArgumentNullException("type");
            var ret = type.GetMethod(method, ALL);
            if (throwOnError && ret == null)
                throw new Exception($"Method not found: {type.Name}.{method}");
            return ret;
        }

        /// <summary>
        /// Invokes static method of any access type.
        /// like: type.method()
        /// </summary>
        /// <param name="method">static method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(Type type, string method) {
            return GetMethod(type, method, true)?.Invoke(null, null);
        }

        /// <summary>
        /// Invokes static method of any access type.
        /// like: qualifiedType.method()
        /// </summary>
        /// <param name="method">static method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(string qualifiedType, string method) {
            var type = Type.GetType(qualifiedType, true);
            return InvokeMethod(type, method);
        }

        /// <summary>
        /// Invokes instance method of any access type.
        /// like: qualifiedType.method()
        /// </summary>
        /// <param name="method">instance method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(object instance, string method) {
            var type = instance.GetType();
            return GetMethod(type, method, true)?.Invoke(instance, null);
        }


        //instance
        internal static T EventToDelegate<T>(object instance, string eventName)
            where T : Delegate {
            return (T)instance
                .GetType()
                .GetField(eventName, ALL)
                .GetValue(instance);
        }

        //static
        internal static T EventToDelegate<T>(Type type, string eventName)
            where T : Delegate {
            return (T)type
                .GetField(eventName, ALL)
                .GetValue(null);
        }

        //instance
        internal static void InvokeEvent(object instance, string eventName, bool verbose = false) {
            var d = GetEventDelegates(instance, eventName);
            if (verbose) Log.Info($"Executing event `{instance.GetType().FullName}.{eventName}` ...");
            ExecuteDelegates(d, verbose);
        }



        //static
        internal static void InvokeEvent(Type type, string eventName, bool verbose = false) {
            var d = GetEventDelegates(type, eventName);
            if (verbose) Log.Info($"Executing event `{type.FullName}.{eventName}` ...");
            ExecuteDelegates(d, verbose);
        }

        //static
        internal static Delegate[] GetEventDelegates(Type type, string eventName) {
            MulticastDelegate eventDelagate =
                (MulticastDelegate)type
                .GetField(eventName, ALL)
                .GetValue(null);
            return eventDelagate.GetInvocationList();
        }

        //instance
        internal static Delegate[] GetEventDelegates(object instance, string eventName) {
            MulticastDelegate eventDelagate =
                (MulticastDelegate)instance.GetType()
                .GetField(eventName, ALL)
                .GetValue(instance);
            return eventDelagate.GetInvocationList();
        }

        internal static void ExecuteDelegates(Delegate[] delegates, bool verbose = false) {
            if (delegates is null) throw new ArgumentNullException("delegates");
            var timer = new Stopwatch();
            foreach (Delegate dlg in delegates) {
                if (dlg == null) continue;
                if (verbose) {
                    Log.Info($"Executing {dlg.Target}:{dlg.Method.Name} ...");
                    timer.Reset(); timer.Start();
                }
                dlg.Method.Invoke(dlg.Target, null);
                if (verbose) {
                    var ms = timer.ElapsedMilliseconds;
                    Log.Info($"Done executing {dlg.Target}:{dlg.Method.Name}! duration={ms:#,0}ms");
                }
            }
        }
    }
}
