using Android.Bluetooth;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.Bluetooth
{
    /// <summary>
    /// Listens for "Add Contact" requests via Bluetooth RFCOMM
    /// </summary>
    public sealed class BluetoothContactRequestListener : IDisposable
    {
        ErebusAddress addr;
        BluetoothServerSocket l;
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
            l = BluetoothAdapter.DefaultAdapter.ListenUsingRfcommWithServiceRecord("ErebusContactRequest", BluetoothUtils.RfcommAddContactService);
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
                    using (var r = new BinaryReader(s.InputStream))
                    using (var w = new BinaryWriter(s.OutputStream))
                    {
                        var _addr = r.ReadErebusAddress();
                        if (h(_addr, s.RemoteDevice.Name))
                        {
                            w.Write(addr);
                            var vkp = new VerificationKeyProvider();
                            var local = vkp.CreatePrivateKey();
                            w.Write(local.Item2.Length);
                            w.Write(local.Item2);
                            vkp.AddKeyPair(addr, local.Item1, r.ReadBytes(r.ReadInt32()));
                            Log.RecordEvent(this, $"Successfully negotiated contact with {_addr} via Bluetooth RFCOMM.", LogEntrySeverity.Info);
                        }
                    }
                }
            });
    }
}