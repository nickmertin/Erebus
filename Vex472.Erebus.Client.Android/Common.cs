using Vex472.Erebus.Android.Bluetooth;
using Vex472.Erebus.Core;

namespace Vex472.Erebus.Client.Android
{
    public static class Common
    {
        public static ErebusInstance Instance { get; set; }

        public static BluetoothConnectionListener BluetoothConnectionListener { get; set; }

        public static BluetoothContactRequestListener BluetoothContactRequestListener { get; set; }

        public static int ConfigurationPin { get; set; }
    }
}