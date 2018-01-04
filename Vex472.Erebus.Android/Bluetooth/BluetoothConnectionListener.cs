using Android.Bluetooth;
using System;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.Bluetooth
{
    /// <summary>
    /// Listens for <see cref="ErebusLink"/> connections via Bluetooth RFCOMM.
    /// </summary>
    public sealed class BluetoothConnectionListener : IDisposable
    {
        ErebusInstance ei;
        BluetoothServerSocket l;
        Task lt;
        bool process = true;

        /// <summary>
        /// Creates a new listener.
        /// </summary>
        /// <param name="instance">The <see cref="ErebusInstance"/> that this listener provides connections to.</param>
        public BluetoothConnectionListener(ErebusInstance instance)
        {
            ei = instance;
            l = BluetoothAdapter.DefaultAdapter.ListenUsingRfcommWithServiceRecord("ErebusConnection", BluetoothUtils.RfcommConnectionService);
            lt = listenProcess();
        }

        /// <summary>
        /// Stops the listener and releases all resources.
        /// </summary>
        public void Dispose()
        {
            process = false;
            l.Dispose();
            lt.Wait();
        }

        Task listenProcess() => Task.Run(() =>
            {
                while (process)
                {
                    var s = l.Accept();
                    new ErebusLink(new SplitStream(s.InputStream, s.OutputStream), ei, true);
                }
            });
    }
}