namespace KianCommons {
    public struct DynamicFlags2 {
        public static readonly ulong[] EMPTY_FLAGS = new ulong[0];
        public static readonly DynamicFlags2 NONE = new DynamicFlags2(EMPTY_FLAGS);

        private ulong[] m_flags;

        public bool IsEmpty {
            get {
                for (int i = 0; i < m_flags.Length; i++) {
                    if (m_flags[i] != 0) {
                        return false;
                    }
                }
                return true;
            }
        }

        public DynamicFlags2(ulong[] flags) {
            m_flags = flags ?? EMPTY_FLAGS;
        }

        public static bool CheckFlags(DynamicFlags2 flags, DynamicFlags2 required, DynamicFlags2 forbidden) {
            int n = System.Math.Max(System.Math.Max(required.m_flags.Length, forbidden.m_flags.Length), flags.m_flags.Length);
            for (int i = 0; i < n; i++) {
                ulong flag = (flags.m_flags.Length <= i) ? 0 : flags.m_flags[i];
                ulong flagRequired = (required.m_flags.Length <= i) ? 0 : required.m_flags[i];
                ulong flagForbidden = (forbidden.m_flags.Length <= i) ? 0 : forbidden.m_flags[i];
                if ((flag & (flagRequired | flagForbidden)) != flagRequired) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAnyFlagSet(DynamicFlags2 flags, DynamicFlags2 flags2) {
            int n = System.Math.Min(flags.m_flags.Length, flags2.m_flags.Length);
            for (int i = 0; i < n; i++) {
                ulong flag = flags.m_flags[i];
                ulong flag2 = flags2.m_flags[i];
                if ((flag & flag2) != 0)
                    return true;
            }
            return false;
        }

        public static bool IsAnyFlagSet(DynamicFlags flags, DynamicFlags flags2) {
            return !DynamicFlags.Check(flags, new DynamicFlags(EMPTY_FLAGS), flags2);
        }


        public static DynamicFlags2 operator &(DynamicFlags2 a, DynamicFlags2 b) {
            int num = System.Math.Max(a.m_flags.Length, b.m_flags.Length);
            ulong[] array = new ulong[num];
            for (int i = 0; i < num; i++) {
                ulong aFlag = (a.m_flags.Length <= i) ? 0 : a.m_flags[i];
                ulong bFlag = (b.m_flags.Length <= i) ? 0 : b.m_flags[i];
                array[i] = aFlag & bFlag;
            }
            return new DynamicFlags2(array);
        }

        public static DynamicFlags2 operator |(DynamicFlags2 a, DynamicFlags2 b) {
            int num = System.Math.Max(a.m_flags.Length, b.m_flags.Length);
            ulong[] array = new ulong[num];
            for (int i = 0; i < num; i++) {
                ulong aFlag = (a.m_flags.Length <= i) ? 0 : a.m_flags[i];
                ulong bFlag = (b.m_flags.Length <= i) ? 0 : b.m_flags[i];
                array[i] = aFlag | bFlag;
            }
            return new DynamicFlags2(array);
        }

        public static DynamicFlags2 operator ^(DynamicFlags2 a, DynamicFlags2 b) {
            int num = System.Math.Max(a.m_flags.Length, b.m_flags.Length);
            ulong[] array = new ulong[num];
            for (int i = 0; i < num; i++) {
                ulong aFlag = (a.m_flags.Length <= i) ? 0 : a.m_flags[i];
                ulong bFlag = (b.m_flags.Length <= i) ? 0 : b.m_flags[i];
                array[i] = aFlag ^ bFlag;
            }
            return new DynamicFlags2(array);
        }

        public static bool operator ==(DynamicFlags2 a, DynamicFlags2 b) {
            int num = System.Math.Max(a.m_flags.Length, b.m_flags.Length);
            for (int i = 0; i < num; i++) {
                ulong aFlag = (a.m_flags.Length <= i) ? 0 : a.m_flags[i];
                ulong bFlag = (b.m_flags.Length <= i) ? 0 : b.m_flags[i];
                if (aFlag != bFlag) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(DynamicFlags2 a, DynamicFlags2 b) => !(a == b);

        public override bool Equals(object obj) => obj is DynamicFlags2 flags2 && flags2.m_flags == m_flags;

        public override int GetHashCode() {
            unchecked {
                const int prime = 31;
                int result = 1; // array length does not matter.
                for (int i = 0; i < m_flags.Length; ++i) {
                    int lsb = (int)(m_flags[i]);
                    int msb = (int)(m_flags[i] >> 32);
                    result = (prime * result) + lsb;
                    result = (prime * result) + msb;
                }

                return result;
            }
        }

        public override string ToString() => $"DynamicFlags2" + m_flags.ToSTR();
    }
}
