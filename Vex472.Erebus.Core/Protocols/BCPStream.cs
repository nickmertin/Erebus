using System;
using System.IO;
using System.Linq;
using Vex472.Erebus.Core.Encryption;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Provides an interface for receive/transmit operations over a BCP connection.
    /// </summary>
    public sealed class BCPStream : Stream
    {
        ThreadSafetyManager stsm = new ThreadSafetyManager(), rtsm = new ThreadSafetyManager();
        Utilities.Buffer sendBuffer = new Utilities.Buffer();
        SignatureProvider sp = new SignatureProvider();
        bool connected = true;
        byte[] frame = new byte[0];
        int framePos = 0;
        Action<byte[]> _send;
        Func<int, byte[]> _recv;

        internal BCPStream(Action<byte[]> send, Func<int, byte[]> recv, byte[] myPrivateKey, byte[] otherPublicKey)
        {
            _send = send;
            _recv = recv;
            sp.Initialize(myPrivateKey, otherPublicKey);
        }

        /// <summary>
        /// Gets whether or not the <see cref="BCPProtocolBase"/> can currently read.
        /// </summary>
        public override bool CanRead => connected;

        /// <summary>
        /// Gets whether or not the <see cref="BCPProtocolBase"/> can currently seek.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets whether or not the <see cref="BCPProtocolBase"/> can currently write.
        /// </summary>
        public override bool CanWrite => connected;

        /// <summary>
        /// Gets the current length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the current position of the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Flushes all data currently buffered for sending.
        /// </summary>
        public override void Flush() => stsm.RunSafe(() =>
            {
                var buf = sendBuffer.GetAsByteArray();
                if (buf.Length == 0)
                    return;
                sendBuffer.Clear();
                Log.RecordEvent(this, $"Sending data: {buf.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}", LogEntrySeverity.Info);
                using (var s = new MemoryStream())
                using (var w = new BinaryWriter(s))
                {
                    w.Write(buf.Length);
                    w.Write(buf);
                    var sig = sp.Sign(buf);
                    w.Write(sig.Length);
                    w.Write(sig);
                    _send(s.ToArray());
                }
            });

        /// <summary>
        /// Reads data from the stream into the given byte array.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The starting position within <paramref name="buffer"/>.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count) => rtsm.RunSafe(() =>
            {
                if (framePos == frame.Length)
                    readFrame();
                count = Math.Min(count, frame.Length - framePos);
                Array.Copy(frame, framePos, buffer, offset, count);
                framePos += count;
                return count;
            });

        /// <summary>
        /// Seeks to a given offset within the stream.
        /// </summary>
        /// <param name="offset">The offset to seek to.</param>
        /// <param name="origin">The origin to seek relative to.</param>
        /// <returns>The new position.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes data to the stream from the given byte array.
        /// </summary>
        /// <param name="buffer">The byte array to write from.</param>
        /// <param name="offset">The starting position withing <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count) => stsm.RunSafe(() =>
            {
                var data = new byte[count];
                Array.Copy(buffer, offset, data, 0, count);
                sendBuffer.Append(data);
                if (sendBuffer.Size > 4096)
                    Flush();
            });

        /// <summary>
        /// Releases the stream's resources.
        /// </summary>
        /// <param name="disposing">True if managed resources are to be released as well as unmanaged resources.</param>
        protected sealed override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            connected = false;
            if (disposing)
            {
                stsm.Dispose();
                sp.Dispose();
            }
        }

        void readFrame()
        {
            var buf = _recv(BitConverter.ToInt32(_recv(4), 0));
            var sig = _recv(BitConverter.ToInt32(_recv(4), 0));
            Log.RecordEvent(this, $"Received data: {buf.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}", LogEntrySeverity.Info);
            if (!sp.Verify(buf, sig))
            {
                Log.RecordEvent(this, "Data received via BCP has been tampered with!", LogEntrySeverity.Error);
                throw new BCPDataTamperedWithException(buf);
            }
            frame = buf;
            framePos = 0;
        }
    }
}