using System;
using System.Collections.Generic;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Provides APIs for recording and handling log entries.
    /// </summary>
    public static class Log
    {
        static List<EventHandler<LogEventArgs>> handlers = new List<EventHandler<LogEventArgs>>();
        static ThreadSafetyManager tsm = new ThreadSafetyManager();

        /// <summary>
        /// Occurs when an entry is recorded in the log.
        /// </summary>
        public static event EventHandler<LogEventArgs> EntryRecorded
        {
            add { handlers.Add(value); }
            remove { handlers.Remove(value); }
        }

        /// <summary>
        /// Records an entry.
        /// </summary>
        /// <param name="sender">The object that created the entry.</param>
        /// <param name="message">The entry's message.</param>
        /// <param name="severity">The severity level of the entry.</param>
        public static void RecordEvent(object sender, string message, LogEntrySeverity severity)
        {
            var e = new LogEventArgs(message, severity);
            tsm.RunSafe(() =>
            {
                foreach (var h in handlers)
                    h(sender, e);
            });
        }
    }
}