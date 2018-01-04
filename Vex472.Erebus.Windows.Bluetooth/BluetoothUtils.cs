using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;

namespace Vex472.Erebus.Windows.Bluetooth
{
    /// <summary>
    /// Provides utilities for connecting and sending contact requests over Bluetooth RFCOMM.
    /// </summary>
    public static class BluetoothUtils
    {
        /// <summary>
        /// The Erebus Connection Bluetooth RFCOMM Service ID.
        /// </summary>
        public static readonly RfcommServiceId RfcommConnectionService = RfcommServiceId.FromUuid(CommonData.RfcommConnectionServiceUuid);

        /// <summary>
        /// The Erebus Add Contact Bluetooth RFCOMM Service ID.
        /// </summary>
        public static readonly RfcommServiceId RfcommAddContactService = RfcommServiceId.FromUuid(CommonData.RfcommAddContactServiceUuid);

        /// <summary>
        /// Lists all available Bluetooth devices that advertise the Erebus Bluetooth RFCOMM Service.
        /// </summary>
        /// <returns>An array of <see cref="RfcommDeviceService"/>s for the eligible bluetooth devices.</returns>
        public static RfcommDeviceService[] EnumerateDevices()
        {
            Log.RecordEvent(typeof(BluetoothUtils), "Valid Bluetooth device list requested.", LogEntrySeverity.Info);
            var result = new List<RfcommDeviceService>();
            new Func<Task>(async () =>
                {
                    foreach (var d in await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommConnectionService)))
                        result.Add(await RfcommDeviceService.FromIdAsync(d.Id));
                })().Wait();
            Log.RecordEvent(typeof(BluetoothUtils), $"There are {result.Count} valid bluetooth devices.", LogEntrySeverity.Info);
            return result.ToArray();
        }

        /// <summary>
        /// Connects to the specified <see cref="RfcommDeviceService"/> and returns a <see cref="Stream"/> of the encrypted connection.
        /// </summary>
        /// <param name="device">The device to connect to.</param>
        /// <returns>The connection stream.</returns>
        public static dynamic Connect(RfcommDeviceService device)
        {
            Log.RecordEvent(typeof(BluetoothUtils), $"Attempting to connect to {device.ConnectionHostName.CanonicalName} via Bluetooth RFCOMM.", LogEntrySeverity.Info);
            var socket = new StreamSocket();
            new Func<Task>(async () =>
                {
                    await socket.ConnectAsync(device.ConnectionHostName, RfcommConnectionService.AsString(), SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                })().Wait();
            Log.RecordEvent(typeof(BluetoothUtils), $"Connection to {device.ConnectionHostName.CanonicalName} via Bluetooth RFCOMM was successful.", LogEntrySeverity.Info);
            return new SplitStream(socket.InputStream.AsStreamForRead(), socket.OutputStream.AsStreamForWrite());
        }

        /// <summary>
        /// Sends a contact request to the specified Bluetooth device.
        /// </summary>
        /// <param name="device">The device to send the request to.</param>
        /// <param name="myAddress">The local address to send to the device.</param>
        /// <returns>The Erebus address of the device if the request was accepted; otherwise null.</returns>
        public static ErebusAddress? RequestAddContact(RfcommDeviceService device, ErebusAddress myAddress)
        {
            Log.RecordEvent(typeof(BluetoothUtils), $"Attempting to add Bluetooth device '{device.ConnectionHostName.CanonicalName}' as a contact.", LogEntrySeverity.Info);
            try
            {
                var socket = new StreamSocket();
                new Func<Task>(async () =>
                    {
                        await socket.ConnectAsync(device.ConnectionHostName, RfcommAddContactService.AsString(), SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                    })().Wait();
                using (var r = new BinaryReader(socket.InputStream.AsStreamForRead()))
                using (var w = new BinaryWriter(socket.OutputStream.AsStreamForWrite()))
                {
                    w.Write(myAddress);
                    var addr = r.ReadErebusAddress();
                    var vkp = PlatformServiceProvider.Create("VerificationKeyProvider");
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