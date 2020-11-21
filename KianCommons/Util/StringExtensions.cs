namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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

        internal static string ToSTR(this InstanceID instanceID)
            => $"{instanceID.Type}:{instanceID.Index}";

        internal static string ToSTR(this KeyValuePair<InstanceID, InstanceID> map)
            => $"[{map.Key.ToSTR()}:{map.Value.ToSTR()}]";

        internal static string ToSTR<T>(this IEnumerable<T> list) {
            if (list == null) return "null";
            string ret = "{ ";
            foreach (T item in list) {
                string s;
                if (item is KeyValuePair<InstanceID, InstanceID> map)
                    s = map.ToSTR();
                else
                    s = item.ToString();
                ret += $"{s}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

        internal static string ToSTR<T>(this IEnumerable<T> list, string format) {
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
    }
}
