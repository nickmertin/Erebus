using System;
using Vex472.Erebus.Core;

namespace Vex472.Erebus.Android.DataModel
{
    /// <summary>
    /// Represents an entry in the "Recents" list of contacts.
    /// </summary>
    public struct RecentEntry
    {
        /// <summary>
        /// The address of the contact that the entry refers to.
        /// </summary>
        public readonly ErebusAddress Address;

        /// <summary>
        /// The time that this contact was last used.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Creates a new instance of the <see cref="RecentEntry"/> structure, with the specified address and timestamp.
        /// </summary>
        /// <param name="addr">The address of the contact that the entry refers to.</param>
        /// <param name="time">The time that this contact was last used.</param>
        public RecentEntry(ErebusAddress addr, DateTime time)
        {
            Address = addr;
            Timestamp = time;
        }
    }
}