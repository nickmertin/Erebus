using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents a link to a remote computer.
    /// </summary>
    public sealed class ErebusLink : IDisposable
    {
        ErebusInstance ci;
        Stream stream;
        Task ioTask;
        bool process = true;
        internal ThreadSafetyManager qtsm = new ThreadSafetyManager();
        internal Queue<Packet> q = new Queue<Packet>();

        /// <summary>
        /// Creates a new instance of the <see cref="ErebusLink"/> class within the specified <see cref="ErebusInstance"/>, and connects via the specified <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">The the underlying connection.</param>
        /// <param name="instance">The current Erebus instance.</param>
        /// <param name="server">True if the connection was established by the other instance connecting to the current instance; otherwise false.</param>
        public ErebusLink(Stream connection, ErebusInstance instance, bool server)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection), "Stream cannot be null!");
            stream = connection;
            if (stream == null)
                throw new ConnectionFailedException(instance.Address, "Connect call returned null stream!");
            if (!stream.CanRead)
                throw new ConnectionFailedException(instance.Address, "Connect call returned unreadable stream!");
            if (!stream.CanWrite)
                throw new ConnectionFailedException(instance.Address, "Connect call returned readonly stream!");
            using (var r = new BinaryReader(connection, Encoding.UTF8, true))
            using (var w = new BinaryWriter(connection, Encoding.UTF8, true))
            {
                w.Write(instance.Address);
                RemoteAddress = r.ReadErebusAddress();
            }
            ci = instance;
            ci.AddLink(this);
            if (server)
                stream.WriteByte(0);
            ioTask = ioProcess();
        }

        /// <summary>
        /// Gets the address of the remote computer.
        /// </summary>
        public ErebusAddress RemoteAddress { get; private set; }

        Task ioProcess() => Task.Run(async () =>
            {
                Log.RecordEvent(this, $"Link open to {RemoteAddress}.", LogEntrySeverity.Info);
                try
                {
                    using (var r = new BinaryReader(stream))
                    using (var w = new BinaryWriter(stream))
                        while (process)
                        {
                            if (r.ReadBoolean())
                                await ci.RecvTSM.RunSafeAsync(() => ci.RecvQueue.Enqueue(new Packet(r.ReadErebusAddress(), r.ReadErebusAddress(), r.ReadUInt16(), r.ReadInt64(), r.ReadBytes(r.ReadInt32()))));
                            await qtsm.RunSafeAsync(() =>
                            {
                                if (q.Count > 0)
                                {
                                    w.Write(true);
                                    Packet p = q.Dequeue();
                                    w.Write(p.Source);
                                    w.Write(p.Destination);
                                    w.Write(p.Port);
                                    w.Write(p.Identifier);
                                    w.Write(p.Data.Length);
                                    w.Write(p.Data);
                                }
                                else
                                    w.Write(false);
                            });
                            await Task.Delay(5);
                        }
                }
                catch (Exception e)
                {
                    Log.RecordEvent(this, $"Exception in transmit/receive thread for link to {RemoteAddress}: {e.Message}; closing link", LogEntrySeverity.Error);
                }
                finally
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => ((IDisposable)this).Dispose());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            });

        /// <summary>
        /// Closes the link and stops it.
        /// </summary>
        void Close() => ((IDisposable)this).Dispose();

        /// <summary>
        /// Closes the link and releases all resources held by it.
        /// </summary>
        void IDisposable.Dispose()
        {
            process = false;
            ioTask.Wait();
            stream?.Dispose();
            ci.RemoveLink(this);
            Log.RecordEvent(this, $"Link to {RemoteAddress} closed", LogEntrySeverity.Info);
        }
    }
}