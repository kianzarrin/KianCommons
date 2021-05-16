namespace KianCommons.Serialization {
    using System;
    using System.IO;
    using KianCommons;

    public class SimpleDataSerializer {
        public readonly Version Version;
        public readonly MemoryStream Stream;

        // make deserailizer
        SimpleDataSerializer(byte[] data) {
            Stream = new MemoryStream(data, false);
            Version = ReadVersion();
        }

        // make serailizer
        SimpleDataSerializer(Version version, int capacity) {
            Stream = new MemoryStream(capacity);
            Assertion.AssertNotNull(version);
            Version = version;
            WriteVersion(version);
        }

        public static SimpleDataSerializer Reader(byte[] data) => new SimpleDataSerializer(data);
        public static SimpleDataSerializer Writer(Version version, int capacity) =>
            new SimpleDataSerializer(version, capacity);

        public byte[] GetBytes() => Stream.ToArray();


        // from ColossalFramework.IO.DataSerializer
        public int ReadInt32() {
            int num = (Stream.ReadByte() & 255) << 24;
            num |= (Stream.ReadByte() & 255) << 16;
            num |= (Stream.ReadByte() & 255) << 8;
            return num | (Stream.ReadByte() & 255);
        }
        public void WriteInt32(int value) {
            Stream.WriteByte((byte)(value >> 24 & 255));
            Stream.WriteByte((byte)(value >> 16 & 255));
            Stream.WriteByte((byte)(value >> 8 & 255));
            Stream.WriteByte((byte)(value & 255));
        }

        public uint ReadUInt32() {
            uint num = (uint)((uint)(Stream.ReadByte() & 255) << 24);
            num |= (uint)((uint)(Stream.ReadByte() & 255) << 16);
            num |= (uint)((uint)(Stream.ReadByte() & 255) << 8);
            return num | (uint)(Stream.ReadByte() & 255);
        }
        public void WriteUInt32(uint value) {
            Stream.WriteByte((byte)(value >> 24 & 255u));
            Stream.WriteByte((byte)(value >> 16 & 255u));
            Stream.WriteByte((byte)(value >> 8 & 255u));
            Stream.WriteByte((byte)(value & 255u));
        }

        public Version ReadVersion() =>
            new Version(ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32());
        public void WriteVersion(Version version) {
            Assertion.AssertNotNull(version);
            WriteInt32(version.Major);
            WriteInt32(version.Minor);
            WriteInt32(version.Build);
            WriteInt32(version.Revision);
        }
    }
}
