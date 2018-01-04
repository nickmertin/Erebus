using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Vex472.Erebus.Core.Utilities;
using System.Threading.Tasks;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Implements Simple Secure Messaging Protocol to send and receive messages between hosts.
    /// </summary>
    public sealed class SSMP : BCPProtocolBase, IDisposable
    {
        BinaryReader r;
        BinaryWriter w;
        Task recvTask;
        bool process = true;
        ThreadSafetyManager tsm = new ThreadSafetyManager();
        List<EventHandler<SSMPMessageReceivedEventArgs>> messageHandlers = new List<EventHandler<SSMPMessageReceivedEventArgs>>();
        static List<EventHandler<SSMPInvitationReceivedEventArgs>> invitationHandlers = new List<EventHandler<SSMPInvitationReceivedEventArgs>>();
        static ThreadSafetyManager htsm = new ThreadSafetyManager();

        /// <summary>
        /// Creates a new instance of the <see cref="SSMP"/> to send and receive messages, and connects to the specified destination.
        /// </summary>
        /// <param name="instance">The current instance.</param>
        /// <param name="dest">The address of the other host.</param>
        public SSMP(ErebusInstance instance, ErebusAddress dest) : base(instance)
        {
            var s = RequestConnection(new ErebusEndpoint(dest, 20000));
            if (s == null)
                throw new IOException($"Connection to {dest} was rejected!");
            r = new BinaryReader(s, Encoding.UTF8, true);
            w = new BinaryWriter(s, Encoding.UTF8, true);
            Connected = true;
            recvTask = recvProcess();
        }

        SSMP(ErebusInstance instance) : base(instance) { }

        /// <summary>
        /// Gets whether or not a connection is open to the remote host.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Registers Simple Secure Messaging Protocol to handle incoming connections on port 20000.
        /// </summary>
        public static void Register() => RegisterProtocol(20000, i => new SSMP(i));

        /// <summary>
        /// Sends a message containing the specified text to the destination host.
        /// </summary>
        /// <param name="text">The content of the message.</param>
        public void SendMessage(string text) => tsm.RunSafe(() =>
            {
                w.WriteString472(text);
                w.Write(false);
                w.Flush();
            });

        /// <summary>
        /// Sends a message containing the specified text with the given attachment to the destination host.
        /// </summary>
        /// <param name="text">The content of the message.</param>
        /// <param name="filename">The file name of the attachment.</param>
        /// <param name="file">The contents of the attachment.</param>
        public void SendMessage(string text, string filename, byte[] file) => tsm.RunSafe(() =>
            {
                w.WriteString472(text);
                w.Write(true);
                w.WriteString472(filename);
                using (var s = new MemoryStream())
                {
                    using (var gzs = new GZipStream(s, CompressionLevel.Optimal))
                        gzs.Write(file, 0, file.Length);
                    file = s.ToArray();
                }
                w.Write(file.Length);
                w.Write(file);
                w.Flush();
            });

        /// <summary>
        /// Closes the connection and releases all associated resources.
        /// </summary>
        public void Dispose()
        {
            process = false;
            tsm.Dispose();
            ((IDisposable)r).Dispose();
            ((IDisposable)w).Dispose();
            recvTask.Wait();
        }

        /// <summary>
        /// Handles an incoming connection request.
        /// </summary>
        /// <param name="ep">The source of the request.</param>
        /// <param name="accept">A function to call to accept the connection request.</param>
        protected override void HandleConnection(ErebusEndpoint ep, Func<Stream> accept) => htsm.RunSafe(() =>
            {
                foreach (var h in htsm.RunSafe(() => invitationHandlers.ToArray()))
                {
                    h(this, new SSMPInvitationReceivedEventArgs(ep.Address, () =>
                    {
                        Connected = true;
                        var s = accept();
                        r = new BinaryReader(s, Encoding.UTF8, true);
                        w = new BinaryWriter(s, Encoding.UTF8, true);
                        return this;
                    }));
                    if (Connected)
                    {
                        recvTask = recvProcess();
                        return;
                    }
                }
            });

        Task recvProcess() => Task.Run(() =>
            {
                while (process)
                {
                    var e = new SSMPMessageReceivedEventArgs(r.ReadString472(), r.ReadBoolean() ? recvFile() : null);
                    foreach (var h in tsm.RunSafe(() => messageHandlers.ToArray()))
                        h(this, e);
                }
            });

        Tuple<string, byte[]> recvFile()
        {
            var name = r.ReadString472();
            using (var os = new MemoryStream())
            {
                using (var s = new MemoryStream(r.ReadBytes(r.ReadInt32())))
                using (var gzs = new GZipStream(s, CompressionMode.Decompress))
                    gzs.CopyTo(os);
                return Tuple.Create(name, os.ToArray());
            }
        }

        /// <summary>
        /// Occurs when an invitation is received.
        /// </summary>
        public static event EventHandler<SSMPInvitationReceivedEventArgs> InvitationReceived
        {
            add { htsm.RunSafe(() => invitationHandlers.Add(value)); }
            remove { htsm.RunSafe(() => invitationHandlers.Remove(value)); }
        }

        /// <summary>
        /// Occurs when a message is received on an active connection.
        /// </summary>
        public event EventHandler<SSMPMessageReceivedEventArgs> MessageReceived
        {
            add { tsm.RunSafe(() => messageHandlers.Add(value)); }
            remove { tsm.RunSafe(() => messageHandlers.Remove(value)); }
        }
    }
}