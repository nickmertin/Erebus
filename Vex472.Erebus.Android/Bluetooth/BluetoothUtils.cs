using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Java.Util;
using System;
using System.IO;
using System.Linq;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.Bluetooth
{
    /// <summary>
    /// Provides utilities for connecting and sending contact requests over Bluetooth RFCOMM.
    /// </summary>
    public static class BluetoothUtils
    {
        /// <summary>
        /// The Erebus Connection Bluetooth RFCOMM Service ID.
        /// </summary>
        public static readonly UUID RfcommConnectionService = UUID.FromString(CommonData.RfcommConnectionServiceUuid.ToString());

        /// <summary>
        /// The Erebus Add Contact Bluetooth RFCOMM Service ID.
        /// </summary>
        public static readonly UUID RfcommAddContactService = UUID.FromString(CommonData.RfcommAddContactServiceUuid.ToString());

        /// <summary>
        /// Lists all available Bluetooth devices that advertise the Erebus Bluetooth RFCOMM Service.
        /// </summary>
        /// <returns>An array of <see cref="BluetoothDevice"/>s for the eligible bluetooth devices.</returns>
        public static BluetoothDevice[] EnumerateDevices()
        {
            Log.RecordEvent(typeof(BluetoothUtils), "Valid Bluetooth device list requested.", LogEntrySeverity.Info);
            var manager = Application.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var result = manager.Adapter.BondedDevices.ToArray();
            //var result = (from d in devs where d.GetUuids().Contains(new ParcelUuid(RfcommConnectionService)) select d).ToArray();
            Log.RecordEvent(typeof(BluetoothUtils), $"There are {result.Length} valid bluetooth devices.", LogEntrySeverity.Info);
            return result;
        }

        /// <summary>
        /// Gets a moniker that can be used to connect to the device.
        /// </summary>
        /// <param name="device">The device to connect to.</param>
        /// <returns>The connection moniker.</returns>
        public static BluetoothConnectionTarget GetConnectionTarget(this BluetoothDevice device) => new BluetoothConnectionTarget(device);

        /// <summary>
        /// Sends a contact request to the specified Bluetooth device.
        /// </summary>
        /// <param name="device">The device to send the request to.</param>
        /// <param name="myAddress">The local address to send to the device.</param>
        /// <returns>The Erebus address of the device if the request was accepted; otherwise null.</returns>
        public static ErebusAddress? RequestAddContact(BluetoothDevice device, ErebusAddress myAddress)
        {
            Log.RecordEvent(typeof(BluetoothUtils), $"Attempting to add Bluetooth device '{device.Name}' as a contact.", LogEntrySeverity.Info);
            try
            {
                var s = device.CreateRfcommSocketToServiceRecord(RfcommAddContactService);
                s.Connect();
                using (var r = new BinaryReader(s.InputStream))
                using (var w = new BinaryWriter(s.OutputStream))
                {
                    w.Write(true);
                    r.ReadBoolean();
                    w.Write(myAddress);
                    var addr = r.ReadErebusAddress();
                    var vkp = new VerificationKeyProvider();
                    var local = vkp.CreatePrivateKey();
                    w.Write(local.Item2.Length);
                    w.Write(local.Item2);
                    vkp.AddKeyPair(addr, local.Item1, r.ReadBytes(r.ReadInt32()));
                    Log.RecordEvent(typeof(BluetoothUtils), $"Successfully negotiated contact with {addr} via Bluetooth RFCOMM.", LogEntrySeverity.Info);
                    return addr;
                }
            }
            catch (Exception e)
            {
                Log.RecordEvent(typeof(BluetoothUtils), $"Exception adding contact: {e.Message}; assuming other user rejected request.", LogEntrySeverity.Error);
                return null;
            }
        }
    }
}