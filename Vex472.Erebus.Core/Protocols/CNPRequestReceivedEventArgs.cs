using System;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Provides data for the <see cref="CNP.RequestReceived"/> event.
    /// </summary>
    public sealed class CNPRequestReceivedEventArgs : EventArgs
    {
        ErebusAddress _addr;
        int _pin;
        Action _accept;

        internal CNPRequestReceivedEventArgs(ErebusAddress addr, int pin, Action accept)
        {
            _addr = addr;
            _pin = pin;
            _accept = accept;
        }

        /// <summary>
        /// Gets the address of the user that the request came from.
        /// </summary>
        public ErebusAddress Address => _addr;

        /// <summary>
        /// Gets the PIN to verify.
        /// </summary>
        public int PIN => _pin;

        /// <summary>
        /// Accepts the request.
        /// </summary>
        public void Accept() => _accept();
    }
}