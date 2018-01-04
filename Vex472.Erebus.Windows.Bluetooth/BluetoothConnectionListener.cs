using System;
using System.IO;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace Vex472.Erebus.Windows.Bluetooth
{
    /// <summary>
    /// Listens for <see cref="ErebusLink"/> connections via Bluetooth RFCOMM.
    /// </summary>
    public sealed class BluetoothConnectionListener : IDisposable
    {
        ErebusInstance ei;
        Task lt;
        bool process = true;

        /// <summary>
        /// Creates a new listener.
        /// </summary>
        /// <param name="instance">The <see cref="ErebusInstance"/> that this listener provides connections to.</param>
        public BluetoothConnectionListener(dynamic instance)
        {
            ei = instance;
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
                var p = await RfcommServiceProvider.CreateAsync(BluetoothUtils.RfcommConnectionService);
                using (var l = new StreamSocketListener())
                {
                    l.ConnectionReceived += (sender, e) => new ErebusLink(new SplitStream(e.Socket.InputStream.AsStreamForRead(), e.Socket.OutputStream.AsStreamForWrite()), ei, true);
                    await l.BindServiceNameAsync(BluetoothUtils.RfcommConnectionService.AsString(), SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                    p.StartAdvertising(l);
                    while (process)
                        await Task.Delay(10);
                    p.StopAdvertising();
                }
            });
    }
}