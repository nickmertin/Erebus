using System;
using System.Linq;
using System.Security.Cryptography;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android
{
    /// <summary>
    /// Provides an interface for accessing the local key pair store.
    /// </summary>
    public sealed class VerificationKeyProvider
    {
        /// <summary>
        /// Retrieves the key pair assigned to the specified address from the store.
        /// </summary>
        /// <param name="addr">The address that the key pair is assigned to.</param>
        /// <returns>The key pair.</returns>
        public Tuple<byte[], byte[]> GetKeyPair(ErebusAddress addr) => (from k in Configuration.TSM.RunSafe(() => Configuration.VerificationKeys) where k.Address == addr select Tuple.Create(k.PrivateKey, k.PublicKey)).First();

        /// <summary>
        /// Adds the specified key pair to the store, assigning it to the specified address.
        /// </summary>
        /// <param name="addr">The address to assign the key pair to.</param>
        /// <param name="myKey">The local private key.</param>
        /// <param name="otherKey">The remote public key.</param>
        public void AddKeyPair(ErebusAddress addr, byte[] myKey, byte[] otherKey) => Configuration.TSM.RunSafe(() => Configuration.VerificationKeys = Configuration.VerificationKeys.Append(new KeyPair(addr, myKey, otherKey)));

        /// <summary>
        /// Removes the key pair that is assigned to the specified address from the store.
        /// </summary>
        /// <param name="addr">The address of the key pair to remove.</param>
        public void RemoveKeyPair(ErebusAddress addr) => Configuration.TSM.RunSafe(() => Configuration.VerificationKeys = (from k in Configuration.VerificationKeys where k.Address != addr select k).ToArray());

        /// <summary>
        /// Creates a new local private and public key that can be added to the store.
        /// </summary>
        /// <returns>The keys.</returns>
        public Tuple<byte[], byte[]> CreatePrivateKey()
        {
            Log.RecordEvent(this, "Creating new private key.", LogEntrySeverity.Info);
            using (var rsa = new RSACryptoServiceProvider(1024))
                return Tuple.Create(rsa.ExportCspBlob(true), rsa.ExportCspBlob(false));
        }
    }
}