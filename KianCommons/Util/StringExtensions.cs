namespace KianCommons {
    using ColossalFramework.Math;
    using ColossalFramework.Packaging;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    internal static class StringExtensions {
        public static string RemoveExtension(this string path) {
            int i = path.LastIndexOf(".");
            return path.Substring(0, i); //drop dot and everything after.
        }

        /// <summary>
        /// returns a new string without r
        /// </summary>
        public static string Remove(this string s, string r) =>
            s.Replace(r, "");

        /// <summary>
        /// returns a new string without c
        /// </summary>
        public static string RemoveChar(this string s, char c) =>
                s.Remove(c.ToString());

        public static string Remove(this string s, params string[] removes) {
            foreach (string r in removes)
                s = s.Remove(r);
            return s;
        }

        public static string RemoveChars(this string s, params char[] chars) {
            foreach (char c in chars)
                s = s.RemoveChar(c);
            return s;
        }

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

        /// <summary>
        /// Like To string but:
        ///  - returns "null" if object is null
        ///  - returns string.ToSTR() if object is string.
        ///  - recursively returns all items as string if object is IEnumerable
        ///  - returns id and type if object is InstanceID
        ///  - returns id and type of both key and value if obj is InstanceID->InstanceID pair
        /// </summary>
        internal static string ToSTR(this object obj) {
            Assertion.AssertStack();
            if (obj is null) return "<null>";
            if (obj is string str)
                return str.ToSTR();
            if (obj is InstanceID instanceID)
                return instanceID.ToSTR();
            if (obj is Bezier3 bezier3)
                return bezier3.ToSTR();
            if (obj is KeyValuePair<InstanceID, InstanceID> map)
                return map.ToSTR();
            if(obj is IDictionary dict)
                return dict.ToSTR();
            if (obj is IEnumerable list)
                return list.ToSTR();
            return obj.ToString();
        }

        /// <summary>
        ///  - returns "null" if string is null
        ///  - returns "empty" if string is empty
        ///  - returns string otherwise.
        /// </summary>
        internal static string ToSTR(this string str) {
            if (str == "") return "<empty>";
            if (str == null) return "<null>";
            return str;
        }

        /// <summary>
        /// returns id and type of InstanceID
        /// </summary>
        internal static string ToSTR(this InstanceID instanceID)
            => $"{instanceID.Type}:{instanceID.Index}";


        internal static string ToSTR(this Bezier3 bezier)
            => $"Bezier[{bezier.a}, {bezier.b}, {bezier.c}, {bezier.d}]";

        /// <summary>
        /// returns id and type of both key and value
        /// </summary>
        internal static string ToSTR(this KeyValuePair<InstanceID, InstanceID> map)
            => $"[{map.Key.ToSTR()}:{map.Value.ToSTR()}]";

        internal static string ToSTR(this IDictionary dict) {
            if (dict is null) return "<null>";
            List<string> terms = new List<string>(); 
            foreach(var key in dict.Keys) {
                var value = dict[key];
                terms.Add($"({key.ToSTR()} : {value.ToSTR()})");
            }
            return $"{{{terms.Join(", ")} }}";
        }

        /// <summary>
        /// returns all items as string
        /// </summary>
        internal static string ToSTR(this IEnumerable list) {
            if (list is Package p) return p.ToString(); // don't print all assets
            if (list == null) return "<null>";
            string ret = "{ ";
            foreach (object item in list) {
                string s;
                if (item is KeyValuePair<InstanceID, InstanceID> map)
                    s = map.ToSTR();
                else
                    s = item?.ToString()?? "<null>";
                ret += $"{s}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

        /// <summary>
        /// prints all items of the list with the given format.
        /// throws exception if T.ToString(format) does not exists.
        /// </summary>
        internal static string ToSTR<T>(this IEnumerable list, string format) {
            if (list == null) return "<null>";
            MethodInfo mToString = typeof(T).GetMethod("ToString", new[] { typeof(string) })
                ?? throw new Exception($"{typeof(T).Name}.ToString(string) was not found");
            var arg = new object[] { format };
            string ret = "{ ";
            foreach (T item in list) {
                var s = mToString.Invoke(item, arg);
                ret += $"{s}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

        internal static string[] Split(this string str, string separator, StringSplitOptions options = StringSplitOptions.None) =>
            str.Split(new[] { separator }, options);

        internal static string[] SplitLines(this string str, StringSplitOptions options = StringSplitOptions.None) =>
            str.Split("\n", options);

        internal static string Join(this IEnumerable<string> str, string separator)
            => string.Join(separator, str.ToArray());

        internal static string JoinLines(this IEnumerable<string> str)
            => str.Join("\n");
        internal static string RemoveEmptyLines(this string str)
            => str.SplitLines(StringSplitOptions.RemoveEmptyEntries).JoinLines();
    }
}
