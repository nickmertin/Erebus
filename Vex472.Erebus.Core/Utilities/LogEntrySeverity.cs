namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Represents the severity of a log entry.
    /// </summary>
    public enum LogEntrySeverity : byte
    {
        /// <summary>
        /// The entry is simply information.
        /// </summary>
        Info = 0,

        /// <summary>
        /// A problem was handled successfully, with no impact on the execution of the application.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// An error, and will have some impact on the execution of the application.
        /// </summary>
        Error = 2,

        /// <summary>
        /// A fatal error occured, and it will cause the application to exit.
        /// </summary>
        Fatal = 3
    }
}