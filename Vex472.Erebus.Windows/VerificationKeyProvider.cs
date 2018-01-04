using PCLCrypto;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Windows
{
    /// <summary>
    /// Provides an interface for accessing the local key pair store.
    /// </summary>
    public sealed class VerificationKeyProvider
    {
        static ThreadSafetyManager tsm = new ThreadSafetyManager();
        static string _path;
        static byte[] _key;

        /// <summary>
        /// Retrieves the key pair assigned to the specified address from the store.
        /// </summary>
        /// <param name="addr">The address that the key pair is assigned to.</param>
        /// <returns>The key pair.</returns>
        public Tuple<byte[], byte[]> GetKeyPair(ErebusAddress addr) => tsm.RunSafe(() =>
            {
                using (var s = File.OpenRead(_path))
                using (var aes = new AesCryptoServiceProvider { Key = _key })
                {
                    using (var r = new BinaryReader(s, Encoding.Default, true))
                        aes.IV = r.ReadBytes(r.ReadInt32());
                    return (from e in read(s, aes) where e.Item1 == addr select Tuple.Create(e.Item2, e.Item3)).DefaultIfEmpty(null).FirstOrDefault();
                }
            });

        /// <summary>
        /// Adds the specified key pair to the store, assigning it to the specified address.
        /// </summary>
        /// <param name="addr">The address to assign the key pair to.</param>
        /// <param name="myKey">The local private key.</param>
        /// <param name="otherKey">The remote public key.</param>
        public void AddKeyPair(ErebusAddress addr, byte[] myKey, byte[] otherKey) => tsm.RunSafe(() =>
            {
                using (var s = File.Open(_path, FileMode.Open, FileAccess.ReadWrite))
                using (var aes = new AesCryptoServiceProvider { Key = _key })
                {
                    using (var r = new BinaryReader(s, Encoding.Default, true))
                        aes.IV = r.ReadBytes(r.ReadInt32());
                    var entries = read(s, aes);
                    s.Position = 0;
                    s.SetLength(0);
                    using (var w = new BinaryWriter(s, Encoding.Default, true))
                    {
                        w.Write(aes.IV.Length);
                        w.Write(aes.IV);
                    }
                    save(entries.Concat(new[] { Tuple.Create(addr, myKey, otherKey) }).ToArray(), s, aes);
                }
            });

        /// <summary>
        /// Removes the key pair that is assigned to the specified address from the store.
        /// </summary>
        /// <param name="addr">The address of the key pair to remove.</param>
        public void RemoveKeyPair(ErebusAddress addr) => tsm.RunSafe(() =>
            {
                using (var s = File.Open(_path, FileMode.Open, FileAccess.ReadWrite))
                using (var aes = new AesCryptoServiceProvider { Key = _key })
                {
                    using (var r = new BinaryReader(s, Encoding.Default, true))
                        aes.IV = r.ReadBytes(r.ReadInt32());
                    var entries = read(s, aes);
                    s.Position = 0;
                    s.SetLength(0);
                    using (var w = new BinaryWriter(s, Encoding.Default, true))
                    {
                        w.Write(aes.IV.Length);
                        w.Write(aes.IV);
                    }
                    save((from e in entries where e.Item1 != addr select e).ToArray(), s, aes);
                }
            });

        /// <summary>
        /// Creates a new local private and public key that can be added to the store.
        /// </summary>
        /// <returns>The keys.</returns>
        public Tuple<byte[], byte[]> CreatePrivateKey()
        {
            var rsa = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(PCLCrypto.AsymmetricAlgorithm.RsaSignPkcs1Sha512);
            var key = rsa.CreateKeyPair(1024);
                return Tuple.Create(key.Export(), key.ExportPublicKey());
        }

        static Tuple<ErebusAddress, byte[], byte[]>[] read(Stream s, AesCryptoServiceProvider aes)
        {
            using (var ws = new WrapperStream(s))
            using (var cs = new System.Security.Cryptography.CryptoStream(ws, aes.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Read))
            using (var r = new BinaryReader(cs))
            {
                Tuple<ErebusAddress, byte[], byte[]>[] entries;
                int count = r.ReadInt32();
                entries = new Tuple<ErebusAddress, byte[], byte[]>[count];
                for (int i = 0; i < count; ++i)
                    entries[i] = Tuple.Create(r.ReadErebusAddress(), r.ReadBytes(r.ReadInt32()), r.ReadBytes(r.ReadInt32()));
                return entries;
            }
        }

        static void save(Tuple<ErebusAddress, byte[], byte[]>[] entries, Stream s, AesCryptoServiceProvider aes)
        {
            using (var ws = new WrapperStream(s))
            using (var cs = new System.Security.Cryptography.CryptoStream(ws, aes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write))
            using (var w = new BinaryWriter(cs))
            {
                w.Write(entries.Length);
                foreach (var e in entries)
                {
                    w.Write(e.Item1);
                    w.Write(e.Item2.Length);
                    w.Write(e.Item2);
                    w.Write(e.Item3.Length);
                    w.Write(e.Item3);
                }
            }
        }

        /// <summary>
        /// Initializes the key pair store.
        /// </summary>
        /// <param name="path">The file path of the store.</param>
        /// <param name="key">The encryption key used to access the store.</param>
        public static void Initialize(string path, string key)
        {
            using (var sha = new SHA256CryptoServiceProvider())
                _key = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
            if (!File.Exists(path))
                using (var s = File.Create(path))
                using (var aes = new AesCryptoServiceProvider { Key = _key })
                {
                    aes.GenerateIV();
                    using (var w = new BinaryWriter(s, Encoding.Default, true))
                    {
                        w.Write(aes.IV.Length);
                        w.Write(aes.IV);
                    }
                    save(new Tuple<ErebusAddress, byte[], byte[]>[0], s, aes);
                }
            _path = path;
        }
    }
}