namespace KianCommons.Math {
    using UnityEngine;
    public static class MathUtil {
        public const float Epsilon = 0.001f;
        public static bool EqualAprox(float a, float b, float error = Epsilon) {
            float diff = a - b;
            return (diff > -error) & (diff < error);
        }

        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
        public static bool IsPow2(long x) => x != 0 && (x & (x - 1)) == 0;

        // these are required to support negative numbers.
        public static bool IsPow2(int x) => x != 0 && (x & (x - 1)) == 0;
        public static bool IsPow2(short x) => x != 0 && (x & (x - 1)) == 0;

        internal static ushort Clamp2U16(int value) => (ushort)Mathf.Clamp(value, 0, ushort.MaxValue);
    }
}
