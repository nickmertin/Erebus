using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vex472.Erebus.Core.Encryption;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Implements the Bidirectional Communication Protocol, and provides a base class for all application-layer protocols that use it.
    /// </summary>
    public abstract class BCPProtocolBase
    {
        static ThreadSafetyManager ptsm = new ThreadSafetyManager(), itsm = new ThreadSafetyManager();
        static Dictionary<ushort, Func<ErebusInstance, BCPProtocolBase>> protocols = new Dictionary<ushort, Func<ErebusInstance, BCPProtocolBase>>();
        static Dictionary<ErebusInstance, InstanceData> instanceData = new Dictionary<ErebusInstance, InstanceData>();
        ErebusInstance ei;
        Guid localId, remoteId;
        byte[] myKey, otherKey;
        ulong sendPos = 0;
        ThreadSafetyManager stsm = new ThreadSafetyManager();
        List<EventHandler> h = new List<EventHandler>();

        /// <summary>
        /// Creates a new instance of the <see cref="BCPProtocolBase"/> class given the current <see cref="ErebusInstance"/>.
        /// </summary>
        /// <param name="instance">The current <see cref="ErebusInstance"/></param>
        protected BCPProtocolBase(ErebusInstance instance)
        {
            localId = Guid.NewGuid();
            ei = instance;
            itsm.RunSafe(() =>
            {
                if (instanceData.ContainsKey(instance))
                    instanceData[instance].TSM.RunSafe(() => ++instanceData[instance].Count);
                else
                    instanceData.Add(instance, new InstanceData());
                instanceData[instance].TSM.RunSafe(() =>
                {
                    instanceData[instance].CurrentPositions.Add(localId, 0);
                    instanceData[instance].Overflow3.Add(localId, new byte?[0]);
                });
            });
        }

        /// <summary>
        /// Registers a new protocol class to handle incoming connection requests on a given port.
        /// </summary>
        /// <typeparam name="T">The protocol class; must be a subclass of <see cref="BCPProtocolBase"/>.</typeparam>
        /// <param name="port">The port that <typeparamref name="T"/> should handle incoming connection requests on.</param>
        /// <param name="factory">A function to create an instance of <typeparamref name="T"/>.</param>
        static protected void RegisterProtocol<T>(ushort port, Func<ErebusInstance, T> factory) where T : BCPProtocolBase
        {
            if (port >> 14 != 1)
                throw new ArgumentOutOfRangeException(nameof(port), "First two bits of the port must be 01!");
            ptsm.RunSafe(() => protocols.Add(port, factory));
        }

        static internal void HandlePacket(Packet packet, ErebusInstance instance)
        {
            try {
                if (packet.Data.Length == 0)
                {
                    Log.RecordEvent(typeof(BCPProtocolBase), $"BCP packet from {packet.Source} is empty.", LogEntrySeverity.Error);
                    return;
                }
                switch (packet.Data[0])
                {
                    case 0:
                        ErebusEndpoint ep = new ErebusEndpoint(packet.Source, packet.Port);
                        if (packet.Data.Length != 17)
                        {
                            Log.RecordEvent(typeof(BCPProtocolBase), $"BCP connection request packet from {ep} is an invalid size!", LogEntrySeverity.Warning);
                        }
                        BCPProtocolBase p;
                        try
                        {
                            p = ptsm.RunSafe(() => protocols[packet.Port])(instance);
                            p.remoteId = new Guid(packet.Data.Skip(1).ToArray());
                        }
                        catch (KeyNotFoundException)
                        {
                            Log.RecordEvent(typeof(BCPProtocolBase), $"No protocol is associated with port {packet.Port}!", LogEntrySeverity.Warning);
                            instance.SendPacket(ep, new byte[] { 2 }.Concat(packet.Data.Skip(1)).ToArray());
                            return;
                        }
                        try
                        {
                            bool accepted = false;
                            p.Endpoint = ep;
                            p.HandleConnection(ep, () =>
                            {
                                accepted = true;
                                instance.SendPacket(ep, new byte[] { 1 }.Concat(p.remoteId.ToByteArray()).Concat(p.localId.ToByteArray()).ToArray());
                                Log.RecordEvent(p, $"Connected to {ep} with local GUID {p.localId} and remote GUID {p.remoteId}", LogEntrySeverity.Info);
                                p.retrieveKeys(packet.Source);
                                return p.createStream(ep);
                            });
                            if (!accepted)
                                instance.SendPacket(ep, new byte[] { 2 }.Concat(p.remoteId.ToByteArray()).ToArray());
                        }
                        catch (Exception e)
                        {
                            Log.RecordEvent(typeof(BCPProtocolBase), $"Error calling connection handler on {p.GetType()}: {e.Message}", LogEntrySeverity.Error);
                        }
                        break;
                    case 3:
                        var i = itsm.RunSafe(() => instanceData[instance]);
                        i.TSM.RunSafe(() =>
                        {
                            Guid id = new Guid(packet.Data.Range(1, 16).ToArray());
                            ulong pos = BitConverter.ToUInt64(packet.Data.Range(17, 8).ToArray(), 0);
                            var data = packet.Data.Skip(25).ToArray();
                            var cp = i.CurrentPositions[id];
                            if (pos < cp)
                                Log.RecordEvent(typeof(BCPProtocolBase), "BCP packet received for known data.", LogEntrySeverity.Warning);
                            else
                            {
                                var oflow = i.Overflow3[id];
                                var newOflow = new byte?[Math.Max((int)((ulong)data.Length + pos - cp), oflow.Length)];
                                Array.Copy(oflow, newOflow, oflow.Length);
                                Array.Copy(data.Cast<byte?>().ToArray(), 0, newOflow, (int)(pos - cp), data.Length);
                                i.Overflow3[id] = newOflow;
                                refresh3(i, id, packet.Port);
                            }
                        });
                        break;
                    default:
                        var itm = itsm.RunSafe(() => instanceData[instance]);
                        itm.TSM.RunSafe(() =>
                        {
                            var data = packet.Data.Skip(17).ToArray();
                            foreach (var c in (from d in itm.Callbacks12 where d.Item1 == packet.Port && d.Item3 == new Guid(packet.Data.Range(1, 16).ToArray()) && d.Item4(packet.Data[0]) select d).ToArray())
                            {
                                itm.Callbacks12.Remove(c);
                                c.Item5(data.Range(0, Math.Min(data.Length, c.Item2)).ToArray());
                                if (c.Item2 <= data.Length)
                                    data = data.Skip(c.Item2).ToArray();
                                else
                                    data = new byte[0];
                                if (data.Length == 0)
                                    break;
                            }
                            if (data.Length != 0)
                                itm.Overflow12.AddLast(new Tuple<ushort, byte, Guid, byte[]>(packet.Port, packet.Data[0], new Guid(packet.Data.Range(1, 16).ToArray()), data));
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Log.RecordEvent(typeof(BCPProtocolBase), $"Exception in BCP main packet handler: {e.Message}", LogEntrySeverity.Error);
            }
        }

        static byte[] waitRecv12(ErebusInstance ei, int max, ushort port, Predicate<byte> type, Guid id)
        {
            byte[] result = null;
            var data = itsm.RunSafe(() => instanceData[ei]);
            using (var ewh = new EventWaitHandle(false, EventResetMode.AutoReset))
                if (data.TSM.RunSafe(() =>
                {
                    var oflow = from x in data.Overflow12 where x.Item1 == port && type(x.Item2) && x.Item3 == id select x;
                    if (oflow.HasElements())
                    {
                        var o = oflow.First();
                        if (o.Item4.Length > max)
                        {
                            result = o.Item4.Range(0, max).ToArray();
                            data.Overflow12.Find(o).Value = Tuple.Create(o.Item1, o.Item2, o.Item3, o.Item4.Skip(max).ToArray());
                        }
                        else
                        {
                            result = o.Item4;
                            data.TSM.RunSafe(() => data.Overflow12.Remove(oflow.First()));
                        }
                    }
                    else
                    {
                        data.Callbacks12.AddLast(Tuple.Create(port, max, id, type, new Action<byte[]>(d =>
                            {
                                result = d;
                                ewh.Set();
                            })));
                        return true;
                    }
                    return false;
                }))
                    ewh.WaitOne();
            return result;
        }

        static byte[] waitRecv12(ErebusInstance ei, int max, ushort port, byte type, Guid id) => waitRecv12(ei, max, port, t => t == type, id);

        static byte[] waitRecv3(ErebusInstance ei, int len, ushort port, Guid id)
        {
            byte[] result = null;
            var data = itsm.RunSafe(() => instanceData[ei]);
            using (var ewh = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                data.TSM.RunSafe(() => data.Callbacks3.AddLast(Tuple.Create(port, len, id, new Action<byte[]>(d =>
                {
                    result = d;
                    ewh.Set();
                }))));
                Task.Run(() => data.TSM.RunSafe(() => refresh3(data, id, port)));
                ewh.WaitOne();
            }
            return result;
        }

        static void refresh3(InstanceData i, Guid id, ushort port)
        {
            var oflow = i.Overflow3[id];
            int count = oflow.FirstIndexOf(null);
            if (count == -1)
                count = oflow.Length;
            int loc = 0;
            foreach (var c in (from d in i.Callbacks3 where d.Item1 == port && d.Item3 == id select d).ToArray())
            {
                if (loc + c.Item2 > count)
                    break;
                c.Item4(oflow.Range(loc, c.Item2).Cast<byte>().ToArray());
                loc += c.Item2;
                i.Callbacks3.Remove(c);
            }
            i.CurrentPositions[id] += (ulong)loc;
            oflow = oflow.Skip(loc).ToArray();
            i.Overflow3[id] = oflow;
        }

        void waitForStreamClose() => Task.Run(() =>
        {
            waitRecv12(ei, 0, Endpoint.Value.Port, 2, localId);
            Endpoint = null;
            lock (h)
                foreach (var _ in h)
                    _(this, new EventArgs());
        });

        Stream createStream(ErebusEndpoint ep) => new EncryptionNegotiator().Negotiate(new BCPStream(data => stsm.RunSafe(() =>
            {
                ei.SendPacket(ep, new byte[] { 3 }.Concat(remoteId.ToByteArray()).Concat(BitConverter.GetBytes(sendPos)).Concat(data).ToArray());
                sendPos += (ulong)data.Length;
            }), l => waitRecv3(ei, l, ep.Port, localId), myKey, otherKey));

        void retrieveKeys(ErebusAddress other)
        {
            var keys = PlatformServiceProvider.Create("VerificationKeyProvider").GetKeyPair(other);
            myKey = keys.Item1;
            otherKey = keys.Item2;
        }

        /// <summary>
        /// Sends a connection request to the given destination on the given port.
        /// </summary>
        /// <param name="ep">The destination.</param>
        /// <returns>A stream for sending and receiving data over the connection, or null if the connection was denied.</returns>
        protected Stream RequestConnection(ErebusEndpoint ep)
        {
            Log.RecordEvent(this, $"Connecting to {ep}...", LogEntrySeverity.Info);
            ei.SendPacket(ep, new byte[] { 0 }.Concat(localId.ToByteArray()).ToArray());
            byte type = 0;
            byte[] recv = waitRecv12(ei, int.MaxValue, ep.Port, b =>
            {
                type = b;
                return b == 1 || b == 2;
            }, localId);
            if (type == 2)
                return null;
            remoteId = new Guid(recv);
            retrieveKeys(ep.Address);
            Log.RecordEvent(this, $"Connected to {ep} with local GUID {localId} and remote GUID {remoteId}", LogEntrySeverity.Info);
            Endpoint = ep;
            return createStream(ep);
        }

        /// <summary>
        /// Handles an incoming connection request.
        /// </summary>
        /// <param name="ep">The source of the request.</param>
        /// <param name="accept">A function to call to accept the request and retrieve a stream for communication.</param>
        abstract protected void HandleConnection(ErebusEndpoint ep, Func<Stream> accept);

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        public void Close()
        {
            if (Endpoint == null)
                throw new InvalidOperationException("No connection is open!");
            ei.SendPacket(Endpoint.Value, new byte[] { 2 }.Concat(remoteId.ToByteArray()).ToArray());
            Log.RecordEvent(this, $"Connection to {Endpoint} closed manually.", LogEntrySeverity.Info);
            Endpoint = null;
            lock (h)
                foreach (var _ in h)
                    _(this, new EventArgs());
        }

        /// <summary>
        /// Gets the current <see cref="ErebusInstance"/>.
        /// </summary>
        public ErebusInstance Instance => ei;

        /// <summary>
        /// Gets the endpoint of the current connection, or <code>null</code> if a connection is not open.
        /// </summary>
        public ErebusEndpoint? Endpoint { get; private set; }

        /// <summary>
        /// Occurs when the connection is closed by the remote host.
        /// </summary>
        public event EventHandler ConnectionClosed
        {
            add { lock (h) h.Add(value); }
            remove { lock (h) h.Remove(value); }
        }

        /// <summary>
        /// Removes this <see cref="BCPProtocolBase"/> instance from static records.
        /// </summary>
        ~BCPProtocolBase()
        {
            itsm.RunSafe(() =>
            {
                var data = instanceData[ei];
                if (data.TSM.RunSafe(() =>
                 {
                     --data.Count;
                     if (data.Count == 0)
                     {
                         instanceData.Remove(ei);
                         return true;
                     }
                     data.Overflow3.Remove(localId);
                     data.CurrentPositions.Remove(localId);
                     return false;
                 }))
                    data.Dispose();
            });
        }

        class InstanceData : IDisposable
        {
            public int Count = 1;
            public LinkedList<Tuple<ushort, int, Guid, Predicate<byte>, Action<byte[]>>> Callbacks12 = new LinkedList<Tuple<ushort, int, Guid, Predicate<byte>, Action<byte[]>>>();
            public LinkedList<Tuple<ushort, int, Guid, Action<byte[]>>> Callbacks3 = new LinkedList<Tuple<ushort, int, Guid, Action<byte[]>>>();
            public LinkedList<Tuple<ushort, byte, Guid, byte[]>> Overflow12 = new LinkedList<Tuple<ushort, byte, Guid, byte[]>>();
            public Dictionary<Guid, byte?[]> Overflow3 = new Dictionary<Guid, byte?[]>();
            public Dictionary<Guid, ulong> CurrentPositions = new Dictionary<Guid, ulong>();
            public ThreadSafetyManager TSM = new ThreadSafetyManager();

            public void Dispose() => TSM.Dispose();
        }
    }
}