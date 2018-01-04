namespace Vex472.Erebus.Core
{
    /// <summary>
    /// Represents an endpoint for communication over an Erebus-based protocol.
    /// </summary>
    public struct ErebusEndpoint
    {
        /// <summary>
        /// The address of the remote computer.
        /// </summary>
        public ErebusAddress Address;
        
        /// <summary>
        /// The port that the listener is connected to on the remote computer.
        /// </summary>
        public ushort Port;

        /// <summary>
        /// Creates a new instance of the <see cref="ErebusAddress"/> structure.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        public ErebusEndpoint(ErebusAddress addr, ushort port)
        {
            Address = addr;
            Port = port;
        }

        /// <summary>
        /// Generates a string representation of the endpoint.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{Address}:{Port}";
    }
}