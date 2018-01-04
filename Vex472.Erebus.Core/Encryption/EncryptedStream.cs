using PCLCrypto;
using System;
using System.IO;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Encryption
{
    /// <summary>
    /// Encrypts and decrypts data being written to and read from the underlying stream using the specified key.
    /// </summary>
    public sealed class EncryptedStream : Stream
    {
        Stream s;
        byte[] rk, wk;
        int rp = 0, wp = 0;
        ThreadSafetyManager wtsm = new ThreadSafetyManager(), rtsm = new ThreadSafetyManager();
        IHashAlgorithmProvider sha = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha512);

        /// <summary>
        /// Creates a new instance of the <see cref="EncryptedStream"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="key">The encryption/decryption key.</param>
        public EncryptedStream(Stream stream, byte[] key)
        {
            s = stream;
            rk = wk = nextKey(key);
        }

        /// <summary>
        /// Gets whether or not the stream can be read from.
        /// </summary>
        public override bool CanRead => s.CanRead;

        /// <summary>
        /// Gets whether or not the stream's position can be set.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets whether or not the stream can be written to.
        /// </summary>
        public override bool CanWrite => s.CanWrite;

        /// <summary>
        /// Gets the length of the stream.
        /// </summary>
        public override long Length => s.Length;

        /// <summary>
        /// Gets or sets the current position of the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return s.Position;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Ensures that all data written to the stream has been sent to the target destination or storage medium.
        /// </summary>
        public override void Flush() => wtsm.RunSafe(s.Flush);

        /// <summary>
        /// Reads and decrypts data from the stream.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count) => rtsm.RunSafe(() =>
            {
                var buf = new byte[count];
                count = s.Read(buf, 0, count);
                for (int i = 0; i < count; ++i)
                {
                    if (rp == rk.Length)
                    {
                        rk = nextKey(rk);
                        rp = 0;
                    }
                    buffer[offset + i] = (byte)(buf[i] ^ rk[rp++]);
                }
                return count;
            });

        /// <summary>
        /// Sets the position of the stream based on a relative offset.
        /// </summary>
        /// <param name="offset">The relative offset.</param>
        /// <param name="origin">The origin of the relative offset.</param>
        /// <returns>The new position of the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Encrypts and writes data to the stream.
        /// </summary>
        /// <param name="buffer">The byte array to write from.</param>
        /// <param name="offset">The position within <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count) => wtsm.RunSafe(() =>
            {
                var buf = new byte[count];
                for (int i = 0; i < count; ++i)
                {
                    if (wp == wk.Length)
                    {
                        wk = nextKey(wk);
                        wp = 0;
                    }
                    buf[i] = (byte)(buffer[offset + i] ^ wk[wp++]);
                }
                s.Write(buf, 0, count);
            });

        byte[] nextKey(byte[] key) => sha.HashData(key);
    }
}