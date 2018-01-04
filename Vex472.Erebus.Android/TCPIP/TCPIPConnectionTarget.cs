using Java.Net;
using System.IO;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core.Encryption;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.TCPIP
{
    /// <summary>
    /// Represents a moniker for a connection via TCP/IP.
    /// </summary>
    public struct TCPIPConnectionTarget : IConnectionTarget
    {
        string _host;
        int _port;

        /// <summary>
        /// Creates a new instance of the <see cref="TCPIPConnectionTarget"/> structure from the specified hostame and the port.
        /// </summary>
        /// <param name="host">The hostname to connect to.</param>
        /// <param name="port">The TCP port to use.</param>
        public TCPIPConnectionTarget(string host, int port)
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Connects to the host.
        /// </summary>
        /// <returns>A stream of the connection.</returns>
        public Stream Connect()
        {
            var s = new Socket(_host, _port);
            return new EncryptionNegotiator().Negotiate(new SplitStream(s.InputStream, s.OutputStream));
        }

        /// <summary>
        /// Serializes the <see cref="TCPIPConnectionTarget"/> into binary format.
        /// </summary>
        /// <returns>The binary format as a byte array.</returns>
        public byte[] Serialize()
        {
            using (var s = new MemoryStream())
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)0);
                w.WriteString472(_host);
                w.Write(_port);
                w.Flush();
                return s.ToArray();
            }
        }
    }
}