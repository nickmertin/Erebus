using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Encryption;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Windows.TCPIP
{
    /// <summary>
    /// Listens for <see cref="ErebusLink"/> connections over TCP/IP.
    /// </summary>
    public sealed class TCPIPConnectionListener : IDisposable
    {
        TcpListener l;
        ErebusInstance ei;
        Task lt;
        bool process = true;

        /// <summary>
        /// Creates a new listener on the specified port that listen on behalf of the specified <see cref="ErebusInstance"/>.
        /// </summary>
        /// <param name="port">The TCP port to listen on.</param>
        /// <param name="instance">The <see cref="ErebusInstance"/> that this listener provides connections to.</param>
        public TCPIPConnectionListener(int port, ErebusInstance instance)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            l = new TcpListener(port);
#pragma warning restore CS0618 // Type or member is obsolete
            ei = instance;
            lt = listenProcess();
        }

        /// <summary>
        /// Stops the listener and releases all of its resources.
        /// </summary>
        public void Dispose()
        {
            process = false;
            l.Stop();
            lt.Wait();
        }

        Task listenProcess() => Task.Run(() =>
            {
                l.Start();
                Log.RecordEvent(this, $"TCP/IP listener started on port {((IPEndPoint)l.LocalEndpoint).Port}.", LogEntrySeverity.Info);
                var n = new EncryptionNegotiator();
                while (process)
                {
                    try
                    {
                        var c = l.AcceptTcpClient();
                        Log.RecordEvent(this, $"Received TCPIP connection request from {c.Client.RemoteEndPoint}.", LogEntrySeverity.Info);
                        new ErebusLink(n.Negotiate(c.GetStream()), ei, true);
                    }
                    catch (Exception e)
                    {
                        Log.RecordEvent(this, $"Exception while setting up incoming TCPIP connection request: {e.Message}", LogEntrySeverity.Error);
                    }
                }
            });
    }
}