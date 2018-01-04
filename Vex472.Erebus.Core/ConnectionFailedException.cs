using System;

namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents an error that occured while connecting to a remote computer.
    /// </summary>
    public sealed class ConnectionFailedException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ConnectionFailedException"/> class, given the address of the remote computer.
        /// </summary>
        /// <param name="addr">The address of the remote computer.</param>
        public ConnectionFailedException(ErebusAddress addr) : base($"Failed to connect to target {addr}!") { TargetAddress = addr; }

        /// <summary>
        /// Creates a new instance of the <see cref="ConnectionFailedException"/> class, given the address of the remote computer, and a message.
        /// </summary>
        /// <param name="addr">The address of the remote computer.</param>
        /// <param name="message">The message of the exception.</param>
        public ConnectionFailedException(ErebusAddress addr, string message) : base(message) { TargetAddress = addr; }

        /// <summary>
        /// Creates a new instance of the <see cref="ConnectionFailedException"/> class, given the address of the remote computer, a message, and an inner exception.
        /// </summary>
        /// <param name="addr">The address of the remote computer.</param>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public ConnectionFailedException(ErebusAddress addr, string message, Exception inner) : base(message, inner) { TargetAddress = addr; }

        /// <summary>
        /// Gets the address of the remote computer.
        /// </summary>
        public ErebusAddress TargetAddress { get; private set; }
    }
}