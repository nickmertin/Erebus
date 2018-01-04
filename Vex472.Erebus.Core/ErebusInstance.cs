using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents an Erebus instance.
    /// </summary>
    public sealed class ErebusInstance : IDisposable
    {
        Task sendTask, recvTask;
        bool process = true;
        ObservableCollection<ErebusLink> links = new ObservableCollection<ErebusLink>();
        ThreadSafetyManager ltsm = new ThreadSafetyManager();
        internal ThreadSafetyManager SendTSM = new ThreadSafetyManager(), RecvTSM = new ThreadSafetyManager();
        internal Queue<Packet> SendQueue = new Queue<Packet>(), RecvQueue = new Queue<Packet>();

        /// <summary>
        /// Creates a new instance of the <see cref="ErebusAddress"/> class.
        /// </summary>
        /// <param name="addr">The address of the computer.</param>
        public ErebusInstance(ErebusAddress addr)
        {
            Address = addr;
            sendTask = sendProcess();
            recvTask = recvProcess();
        }

        Task sendProcess() => Task.Run(async () =>
            {
                Log.RecordEvent(this, $"Send process started for instance at {Address}.", LogEntrySeverity.Info);
                while (process)
                    try
                    {
                        if (SendTSM.RunSafe(() =>
                            {
                                if (SendQueue.Count > 0)
                                {
                                    var p = SendQueue.Dequeue();
                                    if (p.Destination == ErebusAddress.Broadcast)
                                        foreach (var l in ltsm.RunSafe(() => links.ToArray()))
                                            l.qtsm.RunSafe(() => l.q.Enqueue(p));
                                    else
                                    {
                                        ErebusLink link = ltsm.RunSafe(() => (from l in links select l.RemoteAddress).Contains(p.Destination) ? (from l in links where l.RemoteAddress == p.Destination select l).First() : links[new Random().Next(links.Count)]);
                                        link.qtsm.RunSafe(() => link.q.Enqueue(p));
                                    }
                                    Log.RecordEvent(this, $"Transmitted packet {p.Identifier} from {p.Source} to {p.Destination} on port {p.Port} ({p.Data.Length} bytes)", LogEntrySeverity.Info);
                                    return false;
                                }
                                return true;
                            }))
                            await Task.Delay(5);
                    }
                    catch (Exception e)
                    {
                        Log.RecordEvent(this, $"Exception in main instance transmit thread: {e.Message}", LogEntrySeverity.Warning);
                    }
            });

        Task recvProcess() => Task.Run(async () =>
             {
                 var knownPackets = new List<Tuple<ErebusAddress, long>>();
                 Log.RecordEvent(this, $"Receive process started for instance at {Address}.", LogEntrySeverity.Info);
                 while (process)
                     try
                     {
                         if (RecvTSM.RunSafe(() =>
                             {
                                 if (RecvQueue.Count > 0)
                                 {
                                     var p = RecvQueue.Dequeue();
                                     Log.RecordEvent(this, $"Received packet {p.Identifier} from {p.Source} to {p.Destination} on port {p.Port} ({p.Data.Length} bytes)", LogEntrySeverity.Info);
                                     if (knownPackets.Contains(Tuple.Create(p.Source, p.Identifier)))
                                         return false;
                                     if (p.Destination == Address || p.Destination == ErebusAddress.Broadcast)
                                     {
                                         switch (p.Port >> 14)
                                         {
                                             case 0:
                                                 Task.Run(() => RawProtocolBase.HandlePacket(p, this));
                                                 break;
                                             case 1:
                                                 Task.Run(() => BCPProtocolBase.HandlePacket(p, this));
                                                 break;
                                         }
                                     }
                                     if (p.Destination != Address)
                                         SendTSM.RunSafe(() => SendQueue.Enqueue(p));
                                     knownPackets.Add(Tuple.Create(p.Source, p.Identifier));
                                     return false;
                                 }
                                 return true;
                             }))
                             await Task.Delay(5);
                     }
                     catch (Exception e)
                     {
                         Log.RecordEvent(this, $"Exception in main instance receive thread: {e.Message}", LogEntrySeverity.Warning);
                     }
             });

        internal void AddLink(ErebusLink l) => ltsm.RunSafe(() => links.Add(l));

        internal void RemoveLink(ErebusLink l) => ltsm.RunSafe(() => links.Remove(l));

        internal void SendPacket(ErebusEndpoint ep, byte[] data) => SendTSM.RunSafe(() => SendQueue.Enqueue(new Packet(Address, ep.Address, ep.Port, DateTime.Now.Ticks, data)));

        /// <summary>
        /// Stops the instance and releases all resources held by it.
        /// </summary>
        public void Dispose()
        {
            process = false;
            sendTask.Wait();
            recvTask.Wait();
            ltsm.Dispose();
            SendTSM.Dispose();
            RecvTSM.Dispose();
        }

        /// <summary>
        /// Gets an observable collection of <see cref="ErebusLink"/> objects.
        /// </summary>
        public ReadOnlyObservableCollection<ErebusLink> Links => new ReadOnlyObservableCollection<ErebusLink>(links);

        /// <summary>
        /// Gets the address of the current instance.
        /// </summary>
        public ErebusAddress Address { get; private set; }

        /// <summary>
        /// Gets or sets a list of services that this instance provides.
        /// </summary>
        public HDPService[] Services { get; set; }
    }
}