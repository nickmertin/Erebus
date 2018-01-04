using System;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Provides constants that are needed by various consumers of the libary.
    /// </summary>
    public static class CommonData
    {
        /// <summary>
        /// The UUID of the Erebus Connection Bluetooth RFCOMM Service.
        /// </summary>
        public static readonly Guid RfcommConnectionServiceUuid = new Guid("{EC365EC2-9B0F-4F52-A561-B1A1AE3407A5}");

        /// <summary>
        /// The UUID of the Erebus Add Contact Bluetooth RFCOMM Service.
        /// </summary>
        public static readonly Guid RfcommAddContactServiceUuid = new Guid("{7B7DBDED-A2D1-4424-9E2E-96E152E05622}");
    }
}