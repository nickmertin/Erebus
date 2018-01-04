using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents an Erebus packet.
    /// </summary>
    public struct Packet
    {
        /// <summary>
        /// The address of the origin of the packet.
        /// </summary>
        public readonly ErebusAddress Source;

        /// <summary>
        /// The address of final destination of the packet.
        /// </summary>
        public readonly ErebusAddress Destination;

        /// <summary>
        /// The port number of the packet.
        /// </summary>
        public readonly ushort Port;
        
        /// <summary>
        /// A unique identifier of the packet.
        /// </summary>
        public readonly long Identifier;

        /// <summary>
        /// The data contained in the packet.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// Creates a new instance of the <see cref="Packet"/> structure.
        /// </summary>
        /// <param name="src">The address of the origin of the packet.</param>
        /// <param name="dest">The address of final destination of the packet.</param>
        /// <param name="port">The port number of the packet.</param>
        /// <param name="id">The packet identifier (unique to the source instance).</param>
        /// <param name="data">The data contained in the packet.</param>
        public Packet(ErebusAddress src, ErebusAddress dest, ushort port, long id, byte[] data)
        {
            Source = src;
            Destination = dest;
            Port = port;
            Identifier = id;
            Data = data;
        }
    }
}