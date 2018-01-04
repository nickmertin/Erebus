using System;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Thrown when signature verification detects that data that was received over a Biderectional Communication Protocol connection was tampered with.
    /// </summary>
    public sealed class BCPDataTamperedWithException : Exception
    {
        byte[] _data;

        internal BCPDataTamperedWithException(byte[] data) : base("Data received via BCP has been tampered with!")
        {
            _data = data;
        }

        /// <summary>
        /// Gets the data that was tampered with.
        /// </summary>
        public byte[] ModifiedData => _data.Clone() as byte[];
    }
}