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

        internal static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> a) where T : class =>
            a ?? Enumerable.Empty<T>();
        internal static IEnumerable<T?> EmptyIfNull<T>(this IEnumerable<T?> a) where T: struct=>
            a ?? Enumerable.Empty<T?>();


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

        internal static T[] RemoveAt<T>(this T[] array, int index) {
            var list = new List<T>(array);
            list.RemoveAt(index);
            return list.ToArray();
        }


        internal static bool ContainsRef<T>(this IEnumerable<T> list, T element) where T : class {
            foreach (T item in list) {
                if (object.ReferenceEquals(item, element))
                    return true;
            }
            return false;
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
            if(index>=0)
                array[index] = newVal;
        }

        internal static void ReplaceElement(this Array array, object oldVal, object newVal) {
            int index = (array as IList).IndexOf(oldVal);
            if(index >= 0)
                array.SetValue(newVal, index);
        }

        internal static ref T Last<T>(this T[] array) => ref array[array.Length - 1];

        internal static void Swap<T>(this IList<T> list, int i1, int i2) {
            var temp = list[i1];
            list[i1] = list[i2];
            list[i2] = temp;
        }

        internal static TItem MinBy<TItem, TBy>(this IEnumerable<TItem> items, Func<TItem, TBy> selector)
            where TBy : IComparable {
            if (items == null) return default;
            TItem ret = default;
            TBy val = default;
            bool first = true;
            foreach (TItem item in items) {
                TBy val2 = selector(item);
                if (first || val2.CompareTo(val) < 0) {
                    first = false;
                    ret = item;
                    val = val2;
                }
            }
            return ret;
        }
        internal static TItem MaxBy<TItem, TBy>(this IEnumerable<TItem> items, Func<TItem, TBy> selector)
            where TBy : IComparable {
            if (items == null) return default;
            TItem ret = default;
            TBy val = default;
            bool first = true;
            foreach (TItem item in items) {
                TBy val2 = selector(item);
                if (first || val2.CompareTo(val) > 0) {
                    first = false;
                    ret = item;
                    val = val2;
                }
            }
            return ret;
        }
    }
}
