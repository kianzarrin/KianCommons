namespace KianCommons {
    using System;
    using System.Collections;
    using System.Diagnostics;
    internal static class Assertion {
        [Conditional("DEBUG")]
        internal static void AssertDebug(bool con, string m = "") => Assert(con, m);

        [Conditional("DEBUG")]
        internal static void AssertNotNullDebug(object obj, string m = "") => AssertNotNull(obj, m);

        [Conditional("DEBUG")]
        internal static void AssertEqualDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertEqual(a, b, m);

        [Conditional("DEBUG")]
        internal static void AssertNeqDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertNeq(a, b, m);

        [Conditional("DEBUG")]
        internal static void AssertGTDebug<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        [Conditional("DEBUG")]
        internal static void AssertGTEqDebug<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);


        internal static void AssertNotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void AssertEqual<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) == 0, $"expected {a} == {b} | " + m);

        internal static void AssertNeq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) != 0, $"expected {a} != {b} | " + m);

        internal static void AssertGT<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        internal static void AssertGTEq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);

        internal static void NotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void Equal<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) == 0, $"expected {a} == {b} | " + m);

        internal static void Neq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) != 0, $"expected {a} != {b} | " + m);

        internal static void GT<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        internal static void GTEq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);

        internal static void Assert(bool con, string m = "") {
            if (!con) {
                m = "Assertion failed: " + m;
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        internal static void InRange(IList list, int index) {
            NotNull(list);
            Assert(index >= 0 && index < list.Count, $"index={index} Count={list.Count}");
        }
        

        internal static void AssertStack() {
            var frames = new StackTrace().FrameCount;
            //Log.Debug("stack frames=" + frames);
            if (frames > 200) {
                Exception e = new StackOverflowException("stack frames=" + frames);
                Log.Error(e.ToString());
                throw e;
            }
        }

        internal static void InSimulationThread(bool throwOnError = false) {
            if (!Helpers.InSimulationThread()) {
                const string m = "Assertion failed. expected to be simulation thread";
                Log.Error(m);
                if (throwOnError) throw new Exception(m);
            }
        }

        internal static void InMainThread(bool throwOnError = false) {
            if (!Helpers.InMainThread()) {
                const string m = "Assertion failed. expected to be main thread";
                Log.Error(message: m);
                if (throwOnError) throw new Exception(m);

            }
        }
    }
}