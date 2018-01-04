using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents an Erebus address.
    /// </summary>
    public struct ErebusAddress
    {
        /// <summary>
        /// The first segment.
        /// </summary>
        public readonly ushort Seg1;

        /// <summary>
        /// The second segment.
        /// </summary>
        public readonly ushort Seg2;

        /// <summary>
        /// The third segment.
        /// </summary>
        public readonly ushort Seg3;

        /// <summary>
        /// The fourth segment.
        /// </summary>
        public readonly ushort Seg4;

        /// <summary>
        /// The fifth segment.
        /// </summary>
        public readonly ushort Seg5;

        /// <summary>
        /// The sixth segment.
        /// </summary>
        public readonly ushort Seg6;

        /// <summary>
        /// The seventh segment.
        /// </summary>
        public readonly ushort Seg7;

        /// <summary>
        /// The eighth segment.
        /// </summary>
        public readonly ushort Seg8;

        /// <summary>
        /// The universal broadcast address.
        /// </summary>
        public static readonly ErebusAddress Broadcast = new ErebusAddress(65535, 65535, 65535, 65535, 65535, 65535, 65535, 65535);

        /// <summary>
        /// Creates a new instance of the <see cref="ErebusAddress"/> structure with the given segments.
        /// </summary>
        /// <param name="segments">The address's segments</param>
        public ErebusAddress(params ushort[] segments)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments), "Segment array cannot be null!");
            if (segments.Length != 8)
                throw new ArgumentException("Segment array must have 8 elements!", nameof(segments));
            Seg1 = segments[0];
            Seg2 = segments[1];
            Seg3 = segments[2];
            Seg4 = segments[3];
            Seg5 = segments[4];
            Seg6 = segments[5];
            Seg7 = segments[6];
            Seg8 = segments[7];
        }

        /// <summary>
        /// Deserializes an instance of the <see cref="ErebusAddress"/> structure from a given byte array.
        /// </summary>
        /// <param name="data">The binary representation of the address.</param>
        public ErebusAddress(byte[] data) : this(BitConverter.ToUInt16(data, 0), BitConverter.ToUInt16(data, 2), BitConverter.ToUInt16(data, 4), BitConverter.ToUInt16(data, 6), BitConverter.ToUInt16(data, 8), BitConverter.ToUInt16(data, 10), BitConverter.ToUInt16(data, 12), BitConverter.ToUInt16(data, 14)) { }

        /// <summary>
        /// Parses an instance of the <see cref="ErebusAddress"/> structure from a given <see cref="string"/>.
        /// </summary>
        /// <param name="addr">The string representation of the address.</param>
        public ErebusAddress(string addr) : this(getSegments(addr)) { }

        static ushort[] getSegments(string addr)
        {
            if (string.IsNullOrWhiteSpace(addr))
                throw new ArgumentException("String cannot be null, empty, or whitespace!", nameof(addr));
            addr = addr.Trim().ToUpper();
            if (addr[0] != '{' || addr[addr.Length - 1] != '}')
                throw new FormatException("Address must be enclosed within curly braces!");
            var segs = addr.Substring(1, addr.Length - 2).Split('-');
            if (segs.Length < 3)
                throw new FormatException("Address must haveat least 2 hyphens!");
            ushort[] segments;
            if (segs.Length < 7)
            {
                segments = new ushort[8];
                if (segs.Count(s => s == "") != 1)
                    throw new FormatException("Address segmentation is invalid!");
                int split = segs.ToList().IndexOf("");
                for (int i = 0; i < split; ++i)
                    segments[i] = ushort.Parse(segs[i], NumberStyles.AllowHexSpecifier);
                for (int i = segs.Length - split; i < segs.Length; ++i)
                    segments[i - segs.Length + 8] = ushort.Parse(segs[i], NumberStyles.AllowHexSpecifier);
            }
            else if (segs.Length == 8)
                segments = segs.Select(s => ushort.Parse(s, NumberStyles.AllowHexSpecifier)).ToArray();
            else
                throw new FormatException("Address can only have 8 segments!");
            return segments;
        }

        /// <summary>
        /// Deserializes an instance of the <see cref="ErebusAddress"/> structure from a given byte array.
        /// </summary>
        /// <param name="data">The binary representation of the address.</param>
        /// <returns>The <see cref="ErebusAddress"/>.</returns>
        public static ErebusAddress Deserialize(byte[] data) => new ErebusAddress(data);

        /// <summary>
        /// Parses an instance of the <see cref="ErebusAddress"/> from a given <see cref="string"/>.
        /// </summary>
        /// <param name="addr">The textual representation of the address.</param>
        /// <returns>The <see cref="ErebusAddress"/>.</returns>
        public static ErebusAddress Parse(string addr) => new ErebusAddress(addr);

        /// <summary>
        /// Converts an <see cref="ErebusAddress"/> to binary form.
        /// </summary>
        /// <returns>The binary form as a byte array.</returns>
        public byte[] Serialize()
        {
            using (var s = new MemoryStream())
            using (var w = new BinaryWriter(s))
            {
                w.Write(Seg1);
                w.Write(Seg2);
                w.Write(Seg3);
                w.Write(Seg4);
                w.Write(Seg5);
                w.Write(Seg6);
                w.Write(Seg7);
                w.Write(Seg8);
                w.Flush();
                return s.ToArray();
            }
        }

        /// <summary>
        /// Converts the <see cref="ErebusAddress"/> to its textual representation.
        /// </summary>
        /// <returns>The textual representation of the <see cref="ErebusAddress"/>.</returns>
        public override string ToString()
        {
            string result = string.Format("{{{0:X}-{1:X}-{2:X}-{3:X}-{4:X}-{5:X}-{6:X}-{7:X}\f}}", Seg1, Seg2, Seg3, Seg4, Seg5, Seg6, Seg7, Seg8).Replace("\f", "");
            if (Seg1 != 0 && Seg2 != 0 && Seg3 != 0 && Seg4 != 0 && Seg5 != 0 && Seg6 != 0 && Seg7 != 0 && Seg8 != 0)
                return result;
            else
                result = result.Replace("{0-", "{-").Replace("-0}", "-}");
            while (result.Contains("-0-"))
                result = result.Replace("-0-", "--");
            int longest = 0, streak = 0;
            foreach (char c in result)
                if (c == '-')
                    ++streak;
                else
                {
                    if (streak > longest)
                        longest = streak;
                    streak = 0;
                }
            int pos = result.IndexOf(new string(Enumerable.Repeat('-', longest).ToArray()));
            string before = result.Substring(0, pos), after = result.Substring(pos + longest);
            while (before.Contains("--"))
                before = before.Replace("--", "-0-");
            while (after.Contains("--"))
                after = after.Replace("--", "-0-");
            return before + "--" + after;
        }

        /// <summary>
        /// Compares two <see cref="ErebusAddress"/> instances.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Two if the two instances are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ErebusAddress))
                return false;
            ErebusAddress addr = (ErebusAddress)obj;
            return Seg1 == addr.Seg1 && Seg2 == addr.Seg2 && Seg3 == addr.Seg3 && Seg4 == addr.Seg4 && Seg5 == addr.Seg5 && Seg6 == addr.Seg6 && Seg7 == addr.Seg7 && Seg8 == addr.Seg8;
        }

        /// <summary>
        /// Generates a hash code for the <see cref="ErebusAddress"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => ((Seg1 ^ Seg2 ^ Seg3 ^ Seg4) << 16) + (Seg5 ^ Seg6 ^ Seg7 ^ Seg8);

        /// <summary>
        /// Determines whether two specified addresses have the same value.
        /// </summary>
        /// <param name="x">The first address to compare.</param>
        /// <param name="y">The second address to compare.</param>
        /// <returns></returns>
        public static bool operator ==(ErebusAddress x, ErebusAddress y) => x.Equals(y);

        /// <summary>
        /// Determines whether two specified addresses have different values.
        /// </summary>
        /// <param name="x">The first address to compare.</param>
        /// <param name="y">The second address to compare.</param>
        /// <returns></returns>
        public static bool operator !=(ErebusAddress x, ErebusAddress y) => !x.Equals(y);
    }
}