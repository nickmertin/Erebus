using System;
using System.IO;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Vex472.Erebus.Windows.Bluetooth
{
    /// <summary>
    /// Listens for "Add Contact" requests via Bluetooth RFCOMM
    /// </summary>
    public sealed class BluetoothContactRequestListener : IDisposable
    {
        ErebusAddress addr;
        Func<ErebusAddress, string, bool> h;
        Task lt;
        bool process = true;

        /// <summary>
        /// Creates a new listener.
        /// </summary>
        /// <param name="address">The local address to use.</param>
        /// <param name="handler">The function to call when a request is received.</param>
        public BluetoothContactRequestListener(ErebusAddress address, Func<ErebusAddress, string, bool> handler)
        {
            addr = address;
            h = handler;
            lt = listenProcess();
        }

        /// <summary>
        /// Stops the listener and releases all resources.
        /// </summary>
        public void Dispose()
        {
            process = false;
            lt.Wait();
        }

        Task listenProcess() => Task.Run(async () =>
            {
                var p = await RfcommServiceProvider.CreateAsync(BluetoothUtils.RfcommAddContactService);
                using (var l = new StreamSocketListener())
                {
                    l.ConnectionReceived += (sender, e) =>
                    {
                        using (var r = new BinaryReader(e.Socket.InputStream.AsStreamForRead()))
                        using (var w = new BinaryWriter(e.Socket.OutputStream.AsStreamForWrite()))
                        {
                            w.Write(true);
                            r.ReadBoolean();
                            var _addr = r.ReadErebusAddress();
                            if (h(_addr, e.Socket.Information.RemoteHostName.DisplayName))
                            {
                                w.Write(addr);
                                var vkp = PlatformServiceProvider.Create("VerificationKeyProvider");
                                var local = vkp.CreatePrivateKey();
                                w.Write(local.Item2.Length);
                                w.Write(local.Item2);
                                vkp.AddKeyPair(addr, local.Item1, r.ReadBytes(r.ReadInt32()));
                                Log.RecordEvent(this, $"Successfully negotiated contact with {_addr} via Bluetooth RFCOMM.", LogEntrySeverity.Info);
                            }
                        }
                    };
                    await l.BindServiceNameAsync(BluetoothUtils.RfcommAddContactService.AsString(), SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                    using (var w = new DataWriter())
                    {
                        w.WriteByte(37);
                        var name = "Erebus Contact Request Service";
                        w.WriteByte((byte)name.Length);
                        w.UnicodeEncoding = UnicodeEncoding.Utf8;
                        w.WriteString(name);
                        p.SdpRawAttributes.Add(0x100, w.DetachBuffer());
                    }
                    p.StartAdvertising(l);
                    while (process)
                        await Task.Delay(10);
                    p.StopAdvertising();
                }
            });
    }
}