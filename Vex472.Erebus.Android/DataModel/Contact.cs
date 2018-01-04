using Vex472.Erebus.Core;

namespace Vex472.Erebus.Android.DataModel
{
    /// <summary>
    /// Represents an entry in the local contact database.
    /// </summary>
    public struct Contact
    {
        /// <summary>
        /// The contact's local name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The contact's address.
        /// </summary>
        public readonly ErebusAddress Address;

        /// <summary>
        /// Creates a new instance of the <see cref="Contact"/> structure, with the specified name and address.
        /// </summary>
        /// <param name="name">The contact's local name.</param>
        /// <param name="addr">The contact's address.</param>
        public Contact(string name, ErebusAddress addr)
        {
            Name = name;
            Address = addr;
        }
    }
}