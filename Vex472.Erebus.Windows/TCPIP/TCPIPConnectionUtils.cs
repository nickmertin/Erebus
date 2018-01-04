using System.IO;
using System.Net.Sockets;
using Vex472.Erebus.Core.Encryption;

namespace Vex472.Erebus.Windows.TCPIP
{
    /// <summary>
    /// Provides utilities for connecting over TCP/IP.
    /// </summary>
    public static class TCPIPConnectionUtils
    {
        /// <summary>
        /// Connects to a remote <see cref="TCPIPConnectionListener"/> via the given <paramref name="hostname"/> and <paramref name="port"/> via TCP/IP and establishes an encrypted connection.
        /// </summary>
        /// <param name="hostname">The target hostname.</param>
        /// <param name="port">The target port.</param>
        /// <returns>A stream to use the encrypted connection.</returns>
        public static Stream Connect(string hostname, int port) => new EncryptionNegotiator().Negotiate(new TcpClient(hostname, port).GetStream());
    }
}