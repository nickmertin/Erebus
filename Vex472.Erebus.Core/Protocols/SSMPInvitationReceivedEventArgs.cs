using System;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Provides data for the <see cref="SSMP.InvitationReceived"/> event.
    /// </summary>
    public sealed class SSMPInvitationReceivedEventArgs : EventArgs
    {
        Func<SSMP> _accept;

        internal SSMPInvitationReceivedEventArgs(ErebusAddress src, Func<SSMP> accept)
        {
            Source = src;
            _accept = accept;
        }

        /// <summary>
        /// Gets the address of the host that sent the invitation.
        /// </summary>
        public ErebusAddress Source { get; private set; }

        /// <summary>
        /// Accepts the invitation and retrieves a <see cref="SSMP"/> object.
        /// </summary>
        /// <returns>The <see cref="SSMP"/> object for sending and receiving messages.</returns>
        public SSMP Accept() => _accept();
    }
}