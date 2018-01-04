using System;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Represents a message and severity for a log entry.
    /// </summary>
    public sealed class LogEventArgs : EventArgs
    {
        internal LogEventArgs(string message, LogEntrySeverity severity)
        {
            Message = message;
            Severity = severity;
        }

        /// <summary>
        /// Gets the message of the entry.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the severity of the entry.
        /// </summary>
        public LogEntrySeverity Severity { get; private set; }
    }
}