using PCLCrypto;
using Android.App;
using Android.Content;
using System;
using System.IO;
using System.Linq;
using Vex472.Erebus.Android.Bluetooth;
using Vex472.Erebus.Android.TCPIP;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Android.DataModel
{
    /// <summary>
    /// Provides an interface for interacting with the configuration file.
    /// </summary>
    public static class Configuration
    {
        const string FILE_NAME = "erebus_config.dat";
        const ushort FILE_VERSION = 1;

        /// <summary>
        /// Gets the configuration thread safety manager.
        /// </summary>
        public static ThreadSafetyManager TSM { get; } = new ThreadSafetyManager();

        /// <summary>
        /// Gets whether or not the configuration file exists.
        /// </summary>
        public static bool IsConfigured => Application.Context.FileList().Contains(FILE_NAME);

        /// <summary>
        /// Gets or sets the address of the current configuration.
        /// </summary>
        public static ErebusAddress Address { get; set; }

        /// <summary>
        /// Gets or sets the list of targets to connect to.
        /// </summary>
        public static IConnectionTarget[] ConnectionTargets { get; set; }

        /// <summary>
        /// Gets or sets the list of contacts.
        /// </summary>
        public static Contact[] Contacts { get; set; }

        /// <summary>
        /// Gets or sets the list of recent contact entries.
        /// </summary>
        public static RecentEntry[] Recents { get; set; }

        /// <summary>
        /// Gets or sets the list of RSA verification keys.
        /// </summary>
        public static KeyPair[] VerificationKeys { get; set; }

        /// <summary>
        /// Resets the in-memory configuration.
        /// </summary>
        /// <param name="address">The address of the new configuration.</param>
        public static void Reset(ErebusAddress address) => TSM.RunSafe(() =>
            {
                Address = address;
                ConnectionTargets = new IConnectionTarget[0];
                Contacts = new Contact[0];
                Recents = new RecentEntry[0];
                VerificationKeys = new KeyPair[0];
            });

        /// <summary>
        /// Resets the in-memory configuration and saves it to disk with the specified PIN.
        /// </summary>
        /// <param name="address">The address of the new configuration.</param>
        /// <param name="pin">The personal identification number (PIN) to use as an encryption key.</param>
        public static void Configure(ErebusAddress address, int pin) => TSM.RunSafe(() =>
            {
                Reset(address);
                Save(pin);
            });

        /// <summary>
        /// Removes the configuration file.
        /// </summary>
        public static void Unconfigure() => Application.Context.DeleteFile(FILE_NAME);

        /// <summary>
        /// Reloads the in-memory configuration from the configuration file.
        /// </summary>
        /// <param name="pin">The personal identification number (PIN) to use as a decryption key.</param>
        public static void Reload(int pin) => TSM.RunSafe(() =>
            {
                using (var s = Application.Context.OpenFileInput(FILE_NAME))
                using (var r = new BinaryReader(s))
                {
                    var v = r.ReadUInt16();
                    if (v != FILE_VERSION)
                        throw new InvalidDataException($"Config file had version {v}; expected {FILE_VERSION}.");
                    Address = r.ReadErebusAddress();
                    ConnectionTargets = new IConnectionTarget[r.ReadInt32()];
                    for (int i = 0; i < ConnectionTargets.Length; ++i)
                    {
                        var t = r.ReadByte();
                        switch (t)
                        {
                            case 0:
                                ConnectionTargets[i] = new TCPIPConnectionTarget(r.ReadString472(), r.ReadInt32());
                                break;
                            case 1:
                                ConnectionTargets[i] = new BluetoothConnectionTarget(r.ReadString472());
                                break;
                            default:
                                throw new InvalidDataException($"Config file had invalid connection target type {t}.");
                        }
                    }
                    Contacts = new Contact[r.ReadInt32()];
                    for (int i = 0; i < Contacts.Length; ++i)
                        Contacts[i] = new Contact(r.ReadString472(), r.ReadErebusAddress());
                    Recents = new RecentEntry[r.ReadInt32()];
                    for (int i = 0; i < Recents.Length; ++i)
                        Recents[i] = new RecentEntry(r.ReadErebusAddress(), new DateTime(r.ReadInt64()));
                    Log.RecordEvent(typeof(Configuration), "Attempting to decrypt encrypted portion of configuration file.", LogEntrySeverity.Info);
                    var aes = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
                    var sha = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha512);
                    var bk = sha.HashData(BitConverter.GetBytes(pin));
                    var key = bk;
                    var data = r.ReadBytes(r.ReadInt32());
                    for (int i = 0; i < data.Length / 64 + 1; ++i)
                    {
                        key = sha.HashData(key);
                        for (int j = 0; j < 64 && (i < data.Length / 64 || j < data.Length % 64); ++j)
                            data[i * 64 + j] ^= key[j];
                    }
                    using (var ms = new MemoryStream(data))
                    /*using (var es = new EncryptedStream(ms, key))
                    using (var ws = new WrapperStream(ms))
                    using (var cs = new CryptoStream(ws, WinRTCrypto.CryptographicEngine.CreateDecryptor(aes.CreateSymmetricKey(key)), CryptoStreamMode.Read))*/
                    using (var cr = new BinaryReader(ms))
                    {
                        if (!bk.SequenceEqual(cr.ReadBytes(64)))
                            throw new InvalidDataException("Incorrect PIN.");
                        VerificationKeys = new KeyPair[cr.ReadInt32()];
                        for (int i = 0; i < VerificationKeys.Length; ++i)
                            VerificationKeys[i] = new KeyPair(cr.ReadErebusAddress(), cr.ReadBytes(cr.ReadInt32()), cr.ReadBytes(cr.ReadInt32()));
                    }
                    Log.RecordEvent(typeof(Configuration), "Decryption successful.", LogEntrySeverity.Info);
                }
            });

        /// <summary>
        /// Copies the in-memory configuration to the stored configuration file.
        /// </summary>
        /// <param name="pin">The personal identification number (PIN) to use as an encryption key.</param>
        public static void Save(int pin) => TSM.RunSafe(() =>
            {
                using (var s = Application.Context.OpenFileOutput(FILE_NAME, FileCreationMode.Private))
                using (var w = new BinaryWriter(s))
                {
                    w.Write(FILE_VERSION);
                    w.Write(Address);
                    w.Write(ConnectionTargets.Length);
                    foreach (var t in ConnectionTargets)
                        w.Write(t.Serialize());
                    w.Write(Contacts.Length);
                    foreach (var c in Contacts)
                    {
                        w.Write(c.Address);
                        w.WriteString472(c.Name);
                    }
                    w.Write(Recents.Length);
                    foreach (var r in Recents)
                    {
                        w.Write(r.Address);
                        w.Write(r.Timestamp.Ticks);
                    }
                    var aes = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
                    var sha = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha512);
                    var bk = sha.HashData(BitConverter.GetBytes(pin));
                    using (var ms = new MemoryStream())
                    {
                        /*using (var es = new EncryptedStream(ms, key))
                        using (var ws = new WrapperStream(ms))
                        using (var cs = new CryptoStream(ws, WinRTCrypto.CryptographicEngine.CreateEncryptor(aes.CreateSymmetricKey(key)), CryptoStreamMode.Write))*/
                        using (var cw = new BinaryWriter(ms))
                        {
                            cw.Write(bk);
                            cw.Write(VerificationKeys.Length);
                            foreach (var k in VerificationKeys)
                            {
                                w.Write(k.Address);
                                w.Write(k.PrivateKey.Length);
                                w.Write(k.PrivateKey);
                                w.Write(k.PublicKey.Length);
                                w.Write(k.PublicKey);
                            }
                        }
                        var key = bk;
                        var data = ms.ToArray();
                        for (int i = 0; i < data.Length / 64 + 1; ++i)
                        {
                            key = sha.HashData(key);
                            for (int j = 0; j < 64 && (i < data.Length / 64 || j < data.Length % 64); ++j)
                                data[i * 64 + j] ^= key[j];
                        }
                        w.Write(data.Length);
                        w.Write(data);
                    }
                }
            });
    }
}