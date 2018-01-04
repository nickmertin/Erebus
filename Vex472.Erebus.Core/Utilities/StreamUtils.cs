using System;
using System.IO;
using System.Text;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Provides utilities for reading and writing several composite data types to and from streams.
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// Reads a <see cref="string"/> prefixed with a 32-bit signed length from the stream.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> that wraps the stream.</param>
        /// <returns>The <see cref="string"/> that was read.</returns>
        public static string ReadString472(this BinaryReader r)
        {
            int count = r.ReadInt32();
            return Encoding.UTF8.GetString(r.ReadBytes(count), 0, count);
        }

        /// <summary>
        /// Writes a <see cref="string"/> prefixed with a 32-bit signed length from the stream.
        /// </summary>
        /// <param name="w">The <see cref="BinaryWriter"/> that wraps the stream.</param>
        /// <param name="s">The <see cref="string"/> to write.</param>
        public static void WriteString472(this BinaryWriter w, string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            w.Write(b.Length);
            w.Write(b);
        }

        /// <summary>
        /// Reads a <see cref="Guid"/> from the stream.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> that wraps the stream.</param>
        /// <returns>The <see cref="Guid"/> that was read.</returns>
        public static Guid ReadGuid(this BinaryReader r) => new Guid(r.ReadBytes(16));

        /// <summary>
        /// Writes a <see cref="Guid"/> to the stream.
        /// </summary>
        /// <param name="w">The <see cref="BinaryWriter"/> that wraps the stream.</param>
        /// <param name="g">The <see cref="Guid"/> to write.</param>
        public static void Write(this BinaryWriter w, Guid g) => w.Write(g.ToByteArray());

        /// <summary>
        /// Reads a <see cref="Version"/> from the stream.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> that wraps the stream.</param>
        /// <returns>The <see cref="Version"/> that was read.</returns>
        public static Version ReadVersion(this BinaryReader r) => new Version(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());

        /// <summary>
        /// Writes a <see cref="Version"/> to the stream.
        /// </summary>
        /// <param name="w">The <see cref="BinaryWriter"/> that wraps the stream.</param>
        /// <param name="v">The <see cref="Version"/> to write.</param>
        public static void Write(this BinaryWriter w, Version v)
        {
            w.Write(v.Major);
            w.Write(v.Minor);
            w.Write(v.Build);
            w.Write(v.Revision);
        }

        /// <summary>
        /// Reads an <see cref="ErebusAddress"/> from the stream.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> that wraps the stream.</param>
        /// <returns>The <see cref="ErebusAddress"/> that was read.</returns>
        public static ErebusAddress ReadErebusAddress(this BinaryReader r) => new ErebusAddress(r.ReadBytes(16));

        /// <summary>
        /// Writes an <see cref="ErebusAddress"/> to the stream.
        /// </summary>
        /// <param name="w">The <see cref="BinaryWriter"/> that wraps the stream.</param>
        /// <param name="addr">The <see cref="ErebusAddress"/> to write.</param>
        public static void Write(this BinaryWriter w, ErebusAddress addr) => w.Write(addr.Serialize());
    }
}