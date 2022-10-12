namespace KianCommons {
    using System;

    public static class DynamicFlagsUtil {
        public readonly static ulong[] EMPTY_FLAGS = new ulong[0];
        public readonly static string[] EMPTY_TAGS = new string[0];
        public readonly static DynamicFlags NONE = new DynamicFlags(EMPTY_FLAGS);

        public static bool CheckFlags(this DynamicFlags flags, DynamicFlags required, DynamicFlags forbidden) =>
            DynamicFlags.Check(flags, required: required, forbidden: forbidden);

        public static bool IsAnyFlagSet(this DynamicFlags flags, DynamicFlags flags2) =>
            !DynamicFlags.Check(flags, required: new DynamicFlags(EMPTY_FLAGS), forbidden: flags2);

        public static bool CheckFlags(this DynamicFlags2 flags, DynamicFlags2 required, DynamicFlags2 forbidden) =>
            DynamicFlags2.CheckFlags(flags, required: required, forbidden: forbidden);

        public static bool IsAnyFlagSet(this DynamicFlags2 flags, DynamicFlags2 flags2) =>
            !DynamicFlags2.IsAnyFlagSet(flags, flags2);
    }
}
