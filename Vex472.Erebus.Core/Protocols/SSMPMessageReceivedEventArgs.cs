using System;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Provides data for the <see cref="SSMP.MessageReceived"/> event.
    /// </summary>
    public sealed class SSMPMessageReceivedEventArgs : EventArgs
    {
        internal SSMPMessageReceivedEventArgs(string message, Tuple<string, byte[]> attachment)
        {
            Message = message;
            Attachment = attachment;
        }

        /// <summary>
        /// Gets the contents of the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the name and contents of the message's attachment, if there is one.
        /// </summary>
        public Tuple<string, byte[]> Attachment { get; private set; }

        /// <summary>
        /// Gets whether or not the message has an attachment.
        /// </summary>
        public bool HasAttachment => Attachment != null;
    }
}