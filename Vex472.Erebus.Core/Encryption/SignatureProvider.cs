using PCLCrypto;
using System;

namespace Vex472.Erebus.Core.Encryption
{
    /// <summary>
    /// Provides an interface for creating and verifying digital signatures.
    /// </summary>
    public sealed class SignatureProvider : IDisposable
    {
        IHashAlgorithmProvider sha = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha256);
        IAsymmetricKeyAlgorithmProvider rsa = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPkcs1Sha512);
        ICryptographicKey sign, verify;

        /// <summary>
        /// Initializes the <see cref="SignatureProvider"/> with the given key pair.
        /// </summary>
        /// <param name="myPrivateKey">The local private key.</param>
        /// <param name="otherPublicKey">The remote public key.</param>
        public void Initialize(byte[] myPrivateKey, byte[] otherPublicKey)
        {
            sign = rsa.ImportKeyPair(myPrivateKey);
            verify = rsa.ImportPublicKey(otherPublicKey);
        }

        /// <summary>
        /// Signs the specified data using the local private key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The digital signature.</returns>
        public byte[] Sign(byte[] data) => WinRTCrypto.CryptographicEngine.Sign(sign, data);

        /// <summary>
        /// Verifies the specified data and signature using the remote public key.
        /// </summary>
        /// <param name="data">The data to verify.</param>
        /// <param name="signature">The data's signature.</param>
        /// <returns>True if the data is legitimate, otherwise false.</returns>
        public bool Verify(byte[] data, byte[] signature) => WinRTCrypto.CryptographicEngine.VerifySignature(verify, data, signature);

        /// <summary>
        /// Releases the <see cref="SignatureProvider"/>'s resources.
        /// </summary>
        public void Dispose()
        {
            sign.Dispose();
            verify.Dispose();
        }
    }
}