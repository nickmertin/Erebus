using System.IO;
using Vex472.Erebus.Core;

namespace Vex472.Erebus.Android.DataModel
{
    /// <summary>
    /// Represents a moniker for establishing an <see cref="ErebusLink"/>.
    /// </summary>
    public interface IConnectionTarget
    {
        /// <summary>
        /// Connects to the target.
        /// </summary>
        /// <returns>A stream of the connection.</returns>
        Stream Connect();

        /// <summary>
        /// Serializes the moniker into binary form.
        /// </summary>
        /// <returns>The binary form as a byte array.</returns>
        byte[] Serialize();
    }
}