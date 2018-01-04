using Java.Net;
using System;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Encryption;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.TCPIP
{
    /// <summary>
    /// Listens for <see cref="ErebusLink"/> connections over TCP/IP.
    /// </summary>
    public sealed class TCPIPConnectionListener : IDisposable
    {
        ServerSocket l;
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
            l = new ServerSocket(port);
            ei = instance;
            lt = listenProcess();
        }

        /// <summary>
        /// Stops the listener and releases all of its resources.
        /// </summary>
        public void Dispose()
        {
            process = false;
            l.Dispose();
            lt.Wait();
        }

        Task listenProcess() => Task.Run(() =>
            {
                Log.RecordEvent(this, $"TCP/IP listener started on port {l.LocalPort}.", LogEntrySeverity.Info);
                var n = new EncryptionNegotiator();
                while (process)
                {
                    var s = l.Accept();
                    new ErebusLink(n.Negotiate(new SplitStream(s.InputStream, s.OutputStream)), ei, true);
                }
            });
    }
}