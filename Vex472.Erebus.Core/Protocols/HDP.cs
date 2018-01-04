using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Implements Host Discovery Protocol to allow requesting hosts that provide a certain service.
    /// </summary>
    public sealed class HDP : RawProtocolBase, IDisposable
    {
        ThreadSafetyManager htsm = new ThreadSafetyManager();
        List<EventHandler<HDPResponseReceivedEventArgs>> handlers = new List<EventHandler<HDPResponseReceivedEventArgs>>();
        HDPService _service;
        Guid id;
        static ThreadSafetyManager itsm = new ThreadSafetyManager();
        static Dictionary<Guid, HDP> instances = new Dictionary<Guid, HDP>();

        /// <summary>
        /// Creates a new instance of the <see cref="HDP"/> class to request service information from servers.
        /// </summary>
        /// <param name="instance">The current instance.</param>
        /// <param name="service">The service to request.</param>
        public HDP(ErebusInstance instance, HDPService service) : base(instance)
        {
            id = Guid.NewGuid();
            _service = service;
            instances.Add(id, this);
        }

        HDP(ErebusInstance instance) : base(instance)
        {
            id = new Guid();
        }

        /// <summary>
        /// Handles an incoming packet.
        /// </summary>
        /// <param name="packet">The incoming packet.</param>
        protected override void HandlePacket(Packet packet)
        {
            if (id == new Guid())
            {
                if (packet.Data.Length != 18)
                {
                    Log.RecordEvent(this, $"HDP packet from {packet.Source} has invalid size!", LogEntrySeverity.Warning);
                    return;
                }
                if (packet.Data[0] == 1)
                {
                    Guid k = new Guid(packet.Data.Skip(2).ToArray());
                    if (instances.ContainsKey(k))
                        instances[k].HandlePacket(packet);
                    else
                        Log.RecordEvent(this, $"HDP response from {packet.Source} has unknown GUID: {k}", LogEntrySeverity.Warning);
                }
                else if (Instance.Services.Contains((HDPService)packet.Data[1]))
                {
                    byte[] d = packet.Data.Clone() as byte[];
                    d[0] = 1;
                    SendPacket(new ErebusEndpoint(packet.Source, 0), d);
                }
            }
            else
            {
                Log.RecordEvent(this, $"HDP response for service {(HDPService)packet.Data[1]} received from {packet.Source}", LogEntrySeverity.Info);
                htsm.RunSafe(() =>
                    {
                        foreach (var h in handlers)
                            h(this, new HDPResponseReceivedEventArgs(packet.Source, _service));
                    });
            }
        }

        /// <summary>
        /// Gets the service that will be requested.
        /// </summary>
        public HDPService Service => _service;

        /// <summary>
        /// Sends a request to the specified destination.
        /// </summary>
        /// <param name="destination"></param>
        public void SendRequest(ErebusAddress destination)
        {
            SendPacket(new ErebusEndpoint(destination, 0), new byte[] { 0, (byte)_service }.Concat(id.ToByteArray()).ToArray());
            Log.RecordEvent(this, $"HDP request for service {_service} sent to {destination}", LogEntrySeverity.Info);
        }

        /// <summary>
        /// Initiates the closing of the response listener and the subsequent disposing of this instance.
        /// </summary>
        /// <param name="timeout">The length of time to wait before closing, in milliseconds.</param>
        public async void InitClose(int timeout = 30000)
        {
            await Task.Delay(timeout);
            itsm.RunSafe(() => instances.Remove(id));
            ((IDisposable)this).Dispose();
        }

        void IDisposable.Dispose() => htsm.Dispose();

        /// <summary>
        /// Registers Host Discovery Protocol to handle incoming packets on port 0.
        /// </summary>
        public static void Register() => RegisterProtocol(0, i => new HDP(i));

        /// <summary>
        /// Occurs when a response is received from a remote host.
        /// </summary>
        public event EventHandler<HDPResponseReceivedEventArgs> ResponseReceived
        {
            add { handlers.Add(value); }
            remove { handlers.Remove(value); }
        }
    }
}