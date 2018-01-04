using Vex472.Erebus.Core;

namespace Vex472.Erebus.Android.DataModel
{
    /// <summary>
    /// Represents a local private key and remote public key associated with an <see cref="ErebusAddress"/>.
    /// </summary>
    public struct KeyPair
    {
        /// <summary>
        /// The address that the keys are associated with.
        /// </summary>
        public readonly ErebusAddress Address;

        /// <summary>
        /// The local private key.
        /// </summary>
        public readonly byte[] PrivateKey;

        /// <summary>
        /// The remote public key.
        /// </summary>
        public readonly byte[] PublicKey;

        /// <summary>
        /// Creates a new instace of the <see cref="KeyPair"/> structure, with the specified addresss and keys.
        /// </summary>
        /// <param name="addr">The address that the keys are associated with.</param>
        /// <param name="private">The local private key.</param>
        /// <param name="public">The remote public key.</param>
        public KeyPair(ErebusAddress addr, byte[] @private, byte[] @public)
        {
            Address = addr;
            PrivateKey = @private;
            PublicKey = @public;
        }
    }
}