namespace KianCommons {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerationExtensions {
        /// <summary>
        /// returns a new List of cloned items.
        /// </summary>
        internal static List<T> Clone1<T>(this IEnumerable<T> orig) where T : ICloneable =>
            orig.Select(item => (T)item.Clone()).ToList();

        /// <summary>
        /// fast way of determining if collection is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this ICollection<T> a)
            => a == null || a.Count == 0;

        /// <summary>
        /// generic way of determining if IEnumerable is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this IEnumerable<T> a) {
            return a == null || !a.Any();
        }

        internal static int IndexOf<T>(this T[] array, T element) => (array as IList).IndexOf(element);

        internal static void DropElement<T>(ref T[] array, int i) {
            int n1 = array.Length;
            T[] ret = new T[n1 - 1];
            int i1 = 0, i2 = 0;

            while (i1 < n1) {
                if (i1 != i) {
                    ret[i2] = array[i1];
                    i2++;
                }
                i1++;
            }
            array = ret;
        }

        internal static void AppendElement<T>(ref T[] array, T element) {
            int n1 = array.Length;
            T[] ret = new T[n1 + 1];

            for (int i = 0; i < n1; ++i)
                ret[i] = array[i];

            ret.Last() = element;
            array = ret;
        }

        internal static void ReplaceElement<T>(this T[] array, T oldVal, T newVal) {
            int index = (array as IList).IndexOf(oldVal);
            array[index] = newVal;
        }

        internal static void ReplaceElement(this Array array, object oldVal, object newVal) {
            int index = (array as IList).IndexOf(oldVal);
            array.SetValue(newVal, index);
        }

        internal static ref T Last<T>(this T[] array) => ref array[array.Length - 1];

        internal static void Swap<T>(this IList<T> list, int i1, int i2) {
            var temp = list[i1];
            list[i1] = list[i2];
            list[i2] = temp;
        }
    }
}
