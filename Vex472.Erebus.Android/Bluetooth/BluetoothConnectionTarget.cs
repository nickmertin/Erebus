using Android.Bluetooth;
using System.IO;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.Bluetooth
{
    /// <summary>
    /// Represents a moniker for a connection via Bluetooth RFCOMM.
    /// </summary>
    public struct BluetoothConnectionTarget : IConnectionTarget
    {
        BluetoothDevice device;

        /// <summary>
        /// Creates a new instance of the <see cref="BluetoothConnectionTarget"/> structure from the specified <see cref="BluetoothDevice"/>.
        /// </summary>
        /// <param name="dev">The device to connect to.</param>
        public BluetoothConnectionTarget(BluetoothDevice dev)
        {
            device = dev;
        }

        /// <summary>
        /// Create a new instance of the <see cref="BluetoothConnectionTarget"/> structure from the specified address.
        /// </summary>
        /// <param name="address">The address of the device to connect to.</param>
        public BluetoothConnectionTarget(string address) : this(BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address)) { }

        /// <summary>
        /// Connects to the device.
        /// </summary>
        /// <returns>A stream of the connection.</returns>
        public Stream Connect()
        {
            var s = device.CreateRfcommSocketToServiceRecord(BluetoothUtils.RfcommConnectionService);
            s.Connect();
            return new SplitStream(s.InputStream, s.OutputStream);
        }
        
        /// <summary>
        /// Serializes the <see cref="BluetoothConnectionTarget"/> into binary format.
        /// </summary>
        /// <returns>The binary format as a byte array.</returns>
        public byte[] Serialize()
        {
            using (var s = new MemoryStream())
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)1);
                w.WriteString472(device.Address);
                w.Flush();
                return s.ToArray();
            }
        }
    }
}