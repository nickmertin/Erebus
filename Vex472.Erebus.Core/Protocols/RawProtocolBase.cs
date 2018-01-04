using System;
using System.Collections.Generic;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Base class for all application-layer protocols that do not use a transport management protocol.
    /// </summary>
    public abstract class RawProtocolBase
    {
        static ThreadSafetyManager ptsm = new ThreadSafetyManager();
        static Dictionary<ushort, Func<ErebusInstance, RawProtocolBase>> protocols = new Dictionary<ushort, Func<ErebusInstance, RawProtocolBase>>();
        ErebusInstance ei;

        /// <summary>
        /// Creates a new instance of the <see cref="RawProtocolBase"/> class given the current <see cref="ErebusInstance"/>.
        /// </summary>
        /// <param name="instance">The current <see cref="ErebusInstance"/>.</param>
        protected RawProtocolBase(ErebusInstance instance)
        {
            ei = instance;
        }

        /// <summary>
        /// Registers a new protocol class to handle packets on a given port.
        /// </summary>
        /// <typeparam name="T">The protocol class; must be subclass of <see cref="RawProtocolBase"/>.</typeparam>
        /// <param name="port">The port that <typeparamref name="T"/> should handle packets on.</param>
        /// <param name="factory">A function to create an instance of <typeparamref name="T"/>.</param>
        static protected void RegisterProtocol<T>(ushort port, Func<ErebusInstance, T> factory) where T : RawProtocolBase
        {
            if (port >> 14 != 0)
                throw new ArgumentOutOfRangeException(nameof(port), "The first two bits of the port must be 00!");
            ptsm.RunSafe(() => protocols.Add(port, factory));
        }

        static internal void HandlePacket(Packet packet, ErebusInstance instance)
        {
            RawProtocolBase p;
            try
            {
                p = ptsm.RunSafe(() => protocols[packet.Port])(instance);
            }
            catch (KeyNotFoundException)
            {
                Log.RecordEvent(typeof(RawProtocolBase), $"No protocol is associated with port {packet.Port}!", LogEntrySeverity.Warning);
                return;
            }
            try
            {
                p.HandlePacket(packet);
            }
            catch (Exception e)
            {
                Log.RecordEvent(typeof(RawProtocolBase), $"Error calling packet handler on {p.GetType()}: {e.Message}", LogEntrySeverity.Error);
            }
        }

        /// <summary>
        /// Handles an incoming packet.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        abstract protected void HandlePacket(Packet packet);

        /// <summary>
        /// Sends a packet to the specified destination on the specified port.
        /// </summary>
        /// <param name="ep">The destination of the packet.</param>
        /// <param name="data">The contents of the packet.</param>
        protected void SendPacket(ErebusEndpoint ep, byte[] data) => ei.SendPacket(ep, data);

        /// <summary>
        /// Gets the current <see cref="ErebusInstance"/> object.
        /// </summary>
        public ErebusInstance Instance => ei;
    }
}