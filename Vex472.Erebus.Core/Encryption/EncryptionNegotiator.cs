using PCLCrypto;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Encryption
{
    /// <summary>
    /// Provides a service to negotiate encryption over a stream, and create an encrypted stream to host the encryption.
    /// </summary>
    public sealed class EncryptionNegotiator
    {
        /// <summary>
        /// Negotiates encryption on the specified stream.
        /// </summary>
        /// <param name="s">The base stream.</param>
        /// <returns>An encrypted stream that wraps <paramref name="s"/> with encryption.</returns>
        public Stream Negotiate(Stream s)
        {
            using (var r = new BinaryReader(s, Encoding.UTF8, true))
            using (var w = new BinaryWriter(s, Encoding.UTF8, true))
            {
                long diff = 0;
                while (diff == 0)
                {
                    var c = DateTime.Now.Ticks;
                    w.Write(c);
                    w.Flush();
                    diff = c - r.ReadInt64();
                }
                byte[] k;
                if (diff > 0)
                {
                    var rsa = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaPkcs1);
                    var keypair = rsa.CreateKeyPair(1024);
                    k = keypair.ExportPublicKey(CryptographicPublicKeyBlobType.Pkcs1RsaPublicKey);
                    w.Write(k.Length);
                    w.Write(k);
                    s.Flush();
                    k = WinRTCrypto.CryptographicEngine.Decrypt(keypair, (r.ReadBytes(r.ReadInt32())));
                }
                else
                {
                    var rsa = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaPkcs1);
                    var key = rsa.ImportPublicKey(r.ReadBytes(r.ReadInt32()), CryptographicPublicKeyBlobType.Pkcs1RsaPublicKey);
                    k = WinRTCrypto.CryptographicBuffer.GenerateRandom(64);
                    var c = WinRTCrypto.CryptographicEngine.Encrypt(key, k);
                    w.Write(c.Length);
                    w.Write(c);
                }
                Log.RecordEvent(this, $"Encryption has been set up:{k.Select(b => string.Format(" {0:X}", b)).Aggregate((x, y) => x + y)}", LogEntrySeverity.Info);
                return new EncryptedStream(s, k);
            }
        }
    }
}