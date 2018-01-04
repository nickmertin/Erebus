using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Implements Contact Negotiation Protocol to allow user clients to exchange digital signature keys for secure communication over BCP-based protocols.
    /// </summary>
    public sealed class CNP : RawProtocolBase
    {
        static List<EventHandler<CNPRequestReceivedEventArgs>> rrh = new List<EventHandler<CNPRequestReceivedEventArgs>>();
        static Dictionary<Guid, Action<byte[]>> hrh = new Dictionary<Guid, Action<byte[]>>();
        static Dictionary<Guid, Action<bool>> arh = new Dictionary<Guid, Action<bool>>();

        /// <summary>
        /// Creates a new instance of the <see cref="CNP"/> class that uses the specified <see cref="ErebusInstance"/>.
        /// </summary>
        /// <param name="instance">The current instance.</param>
        public CNP(ErebusInstance instance) : base(instance) { }

        /// <summary>
        /// Sends an add contact request to the user at the specified address.
        /// </summary>
        /// <param name="addr">The address of the remote user.</param>
        /// <param name="pinCheck">A predicate function to use to </param>
        /// <returns>True if the request was accepted; otherwise false.</returns>
        public bool RequestContact(ErebusAddress addr, Predicate<int> pinCheck) => core(addr, true, Guid.NewGuid(), null, pinCheck);

        /// <summary>
        /// Handles an incoming packet.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        protected override void HandlePacket(Packet packet)
        {
            if (packet.Data.Length == 0)
            {
                Log.RecordEvent(this, $"CNP packet from {packet.Source} is empty!", LogEntrySeverity.Error);
                return;
            }
            switch (packet.Data[0])
            {
                case 0:
                    if (packet.Data.Length < 17)
                    {
                        Log.RecordEvent(this, $"CNP request packet from {packet.Source} has invalid length {packet.Data.Length}!", LogEntrySeverity.Error);
                        return;
                    }
                    core(packet.Source, false, new Guid(packet.Data.Range(1, 16).ToArray()), packet.Data.Skip(17).ToArray(), pin =>
                        {
                            var l = false;
                            lock (rrh)
                            foreach (var h in rrh)
                                {
                                    try
                                    {
                                        h(this, new CNPRequestReceivedEventArgs(packet.Source, pin, () => l = true));
                                        if (l)
                                            return true;
                                    }
                                    catch (Exception e)
                                    {
                                        Log.RecordEvent(this, $"Exception calling CNP request handler {h}: {e.Message}", LogEntrySeverity.Error);
                                    }
                                }
                            return false;
                        });
                    break;
                case 1:
                    if (packet.Data.Length < 17)
                    {
                        Log.RecordEvent(this, $"CNP handshake response packet from {packet.Source} has invalid length {packet.Data.Length}!", LogEntrySeverity.Error);
                        return;
                    }
                    lock (hrh)
                    {
                        var id = new Guid(packet.Data.Range(1, 16).ToArray());
                        if (!hrh.ContainsKey(id))
                        {
                            Log.RecordEvent(this, $"CNP handshake response packet from {packet.Source} has invalid GUID {id}!", LogEntrySeverity.Error);
                            return;
                        }
                        hrh[id](packet.Data.Skip(17).ToArray());
                    }
                    break;
                case 2:
                case 3:
                    var accept = packet.Data[0] == 2;
                    if (packet.Data.Length != 17)
                    {
                        Log.RecordEvent(this, $"CNP {(accept ? "accept" : "decline")} packet from {packet.Source} has invalid length {packet.Data.Length}!", LogEntrySeverity.Error);
                        return;
                    }
                    lock (arh)
                    {
                        var id = new Guid(packet.Data.Range(1, 16).ToArray());
                        if (!arh.ContainsKey(id))
                        {
                            Log.RecordEvent(this, $"CNP {(accept ? "accept" : "decline")} packet from {packet.Source} has invalid GUID {id}!", LogEntrySeverity.Error);
                            return;
                        }
                        arh[id](accept);
                    }
                    break;
            }
        }

        bool core(ErebusAddress addr, bool request, Guid id, byte[] otherKey, Predicate<int> pinCheck)
        {
            var vkp = PlatformServiceProvider.Create("VerificationKeyProvider");
            var k = vkp.CreatePrivateKey();
            byte[] lp = k.Item2;
            using (var ewh = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                bool l = false, r = false;
                lock (arh)
                    arh[id] = a =>
                    {
                        r = a;
                        ewh.Set();
                        lock (arh)
                            arh.Remove(id);
                    };
                if (request)
                    using (var kewh = new EventWaitHandle(false, EventResetMode.ManualReset))
                    {
                        lock (hrh)
                            hrh[id] = _k =>
                            {
                                otherKey = _k;
                                kewh.Set();
                            };
                        SendPacket(new ErebusEndpoint(addr, 1), new byte[] { 0 }.Concat(id.ToByteArray()).Concat(lp).ToArray());
                        kewh.WaitOne();
                    }
                else
                    SendPacket(new ErebusEndpoint(addr, 1), new byte[] { 1 }.Concat(id.ToByteArray()).Concat(lp).ToArray());
                var hash = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha512).HashData((request ? lp.Concat(otherKey) : otherKey.Concat(lp)).ToArray());
                for (int i = 4; i < hash.Length; ++i)
                    hash[i % 4] ^= hash[i];
                l = pinCheck(BitConverter.ToInt32(hash, 0));
                SendPacket(new ErebusEndpoint(addr, 1), new[] { (byte)(l ? 2 : 3) }.Concat(id.ToByteArray()).ToArray());
                if (l)
                {
                    ewh.WaitOne();
                    if (r)
                    {
                        vkp.AddKeyPair(addr, k.Item1, otherKey);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Registers Contact Negotiation Protocol to handle incoming packets on port 1.
        /// </summary>
        public static void Register() => RegisterProtocol(1, i => new CNP(i));

        /// <summary>
        /// Occurs when an add contact request is received.
        /// </summary>
        public static event EventHandler<CNPRequestReceivedEventArgs> RequestReceived
        {
            add { lock (rrh) rrh.Add(value); }
            remove { lock (rrh) rrh.Remove(value); }
        }
    }
}