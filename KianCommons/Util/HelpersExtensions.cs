using static KianCommons.Assertion;

namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using ICities;
    using System.Diagnostics;
    using System.Reflection;

    internal static class EnumBitMaskExtensions {
        internal static int String2Enum<T>(string str) where T : Enum {
            return Array.IndexOf(Enum.GetNames(typeof(T)), str);
        }

        internal static T Max<T>()
            where T : Enum =>
            Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static void SetBit(this ref byte b, int idx) => b |= (byte)(1 << idx);
        internal static void ClearBit(this ref byte b, int idx) => b &= ((byte)~(1 << idx));
        internal static bool GetBit(this byte b, int idx) => (b & (byte)(1 << idx)) != 0;
        internal static void SetBit(this ref byte b, int idx, bool value) {
            if (value)
                b.SetBit(idx);
            else
                b.ClearBit(idx);
        }

        internal static T GetMaxEnumValue<T>() =>
            System.Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static int GetEnumCount<T>() =>
            System.Enum.GetValues(typeof(T)).Length;

        private static void CheckEnumWithFlags<T>() {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
            }
            if (!Attribute.IsDefined(typeof(T), typeof(FlagsAttribute))) {
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
            }
        }

        internal static bool CheckFlags(this NetNode.Flags value, NetNode.Flags required, NetNode.Flags forbidden) =>
            (value & (required | forbidden)) == required;


        internal static bool CheckFlags(this NetSegment.Flags value, NetSegment.Flags required, NetSegment.Flags forbidden) =>
            (value & (required | forbidden)) == required;

        internal static bool CheckFlags(this NetLane.Flags value, NetLane.Flags required, NetLane.Flags forbidden) =>
            (value & (required | forbidden)) == required;
    }

    internal static class AssemblyTypeExtensions {
        internal static Version Version(this Assembly asm) =>
          asm.GetName().Version;

        internal static Version VersionOf(this Type t) =>
            t.Assembly.GetName().Version;

        internal static Version VersionOf(this object obj) =>
            VersionOf(obj.GetType());

        internal static void CopyProperties(object target, object origin) {
            var t1 = target.GetType();
            var t2 = origin.GetType();
            Assert(t1 == t2 || t1.IsSubclassOf(t2));
            FieldInfo[] fields = origin.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Extensions.Log($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                string strValue = value?.ToString() ?? "null";
                //Extensions.Log($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Extensions.Log($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void CopyProperties<T>(object target, object origin) {
            Assert(target is T, "target is T");
            Assert(origin is T, "origin is T");
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Extensions.Log($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Extensions.Log($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Extensions.Log($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }
    }

    internal static class StringExtensions {
        /// <summary>
        /// returns false if string is null or empty. otherwise returns true.
        /// </summary>
        internal static bool ToBool(this string str) => !(str == null || str == "");

        internal static string STR(this object obj) => obj == null ? "<null>" : obj.ToString();

        internal static string STR(this InstanceID instanceID) =>
            instanceID.Type + ":" + instanceID.Index;

        internal static string BIG(string m) {
            string mul(string s, int i) {
                string ret_ = "";
                while (i-- > 0) ret_ += s;
                return ret_;
            }
            m = "  " + m + "  ";
            int n = 120;
            string stars1 = mul("*", n);
            string stars2 = mul("*", (n - m.Length) / 2);
            string ret = stars1 + "\n" + stars2 + m + stars2 + "\n" + stars1;
            return ret;
        }


        internal static string CenterString(this string stringToCenter, int totalLength) {
            int leftPadding = ((totalLength - stringToCenter.Length) / 2) + stringToCenter.Length;
            return stringToCenter.PadLeft(leftPadding).PadRight(totalLength);
        }

        internal static string ToSTR<T>(this IEnumerable<T> list) {
            string ret = "{ ";
            foreach (T item in list) {
                ret += $"{item}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

    }

    internal static class Assertion {
        internal static void AssertNotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void AssertEqual(int a, int b, string m = "") =>
            Assert(a == b, "expected {a} == {b} | " + m);

        internal static void AssertNeq(int a, int b, string m = "") =>
            Assert(a != b, "expected {a} != {b} | " + m);

        internal static void Assert(bool con, string m = "") {
            if (!con) {
                m = "Assertion failed: " + m;
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        internal static void AssertStack() {
            var frames = new StackTrace().FrameCount;
            //Log.Debug("stack frames=" + frames);
            if (frames > 100) {
                Exception e = new StackOverflowException("stack frames=" + frames);
                Log.Error(e.ToString());
                throw e;
            }
        }
    }

    //internal static class StackHelpers {
    //}

    internal static class HelpersExtensions
    {
        internal static bool InSimulationThread() =>
            System.Threading.Thread.CurrentThread == SimulationManager.instance.m_simulationThread;

        internal static bool VERBOSE = false;

        internal static bool[] ALL_BOOL = new bool[] { false, true};
         
        internal static AppMode currentMode => SimulationManager.instance.m_ManagersWrapper.loading.currentMode;
        internal static bool CheckGameMode(AppMode mode)
        {
            try
            {
                if (currentMode == mode)
                    return true;
            }
            catch { }
            return false;
        }
        internal static bool InGame => CheckGameMode(AppMode.Game);
        internal static bool InAssetEditor => CheckGameMode(AppMode.AssetEditor);
        internal static bool IsActive =>
#if DEBUG
            InGame || InAssetEditor;
#else
            InGame;
#endif

        /// <summary>
        /// returns a new List calling Clone() on all items.
        /// </summary>
        internal static List<T> Clone1<T>(this IList<T> listToClone) where T : ICloneable =>
            listToClone.Select(item => (T)item.Clone()).ToList();

        /// <summary>
        /// returns a new List copying all item
        /// </summary>
        internal static List<T> Clone0<T>(this IList<T> listToClone) =>
            listToClone.Select(item=>item).ToList();


        internal static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        internal static bool ControlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        internal static bool AltIsPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);


    }
}
