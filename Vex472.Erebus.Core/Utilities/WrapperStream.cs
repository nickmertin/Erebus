using System.IO;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Wraps an underlying <see cref="Stream"/>, passing through all I/O calls, but blocking <see cref="Stream.Dispose()"/>.
    /// </summary>
    public sealed class WrapperStream : Stream
    {
        Stream s;

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperStream"/> class that wraps the specified base stream.
        /// </summary>
        /// <param name="baseStream">The stream to wrap.</param>
        public WrapperStream(Stream baseStream)
        {
            s = baseStream;
        }

        /// <summary>
        /// Gets the base stream that is being wrapped.
        /// </summary>
        public Stream BaseStream => s;

        /// <summary>
        /// Gets whether or not the stream can be read from.
        /// </summary>
        public override bool CanRead => s.CanRead;

        /// <summary>
        /// Gets whether or not the position of the stream can be set.
        /// </summary>
        public override bool CanSeek => s.CanSeek;

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
                s.Position = value;
            }
        }

        /// <summary>
        /// Flushes all buffered data that has been written to the stream.
        /// </summary>
        public override void Flush() => s.Flush();

        /// <summary>
        /// Reads data from the stream.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The position withing <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count) => s.Read(buffer, offset, count);

        /// <summary>
        /// Sets the current position of the stream.
        /// </summary>
        /// <param name="offset">The offset of the new position.</param>
        /// <param name="origin">The position that <paramref name="offset"/> is relative to.</param>
        /// <returns>The new position of the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin) => s.Seek(offset, origin);

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value) => s.SetLength(value);

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer">The byte array to write from.</param>
        /// <param name="offset">The position withing <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count) => s.Write(buffer, offset, count);
    }
}