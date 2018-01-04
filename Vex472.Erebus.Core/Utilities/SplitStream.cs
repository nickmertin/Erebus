using System;
using System.IO;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Combines a readable stream and a writeable stream to a single interface for I/O.
    /// </summary>
    public sealed class SplitStream : Stream
    {
        Stream i, o;

        /// <summary>
        /// Creates a new instance of the <see cref="SplitStream"/> class.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <param name="output">The stream to write to.</param>
        public SplitStream(Stream input, Stream output)
        {
            i = input;
            o = output;
        }

        /// <summary>
        /// Gets whether or not the stream can be read from.
        /// </summary>
        public override bool CanRead => i.CanRead;

        /// <summary>
        /// Gets whether or not the position in the stream can be set.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets whether or not the stream can be written to.
        /// </summary>
        public override bool CanWrite => o.CanWrite;

        /// <summary>
        /// Gets the length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the current position of the stream.
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
        /// Flushes the output stream.
        /// </summary>
        public override void Flush() => o.Flush();

        /// <summary>
        /// Reads from the input stream.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The possition within <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes that were read.</returns>
        public override int Read(byte[] buffer, int offset, int count) => i.Read(buffer, offset, count);

        /// <summary>
        /// Sets the position of the stream.
        /// </summary>
        /// <param name="offset">The offset, in bytes, from <paramref name="origin"/>.</param>
        /// <param name="origin">The position that <paramref name="offset"/> is relative to.</param>
        /// <returns></returns>
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
        /// Writes to the output stream.
        /// </summary>
        /// <param name="buffer">The byte array to write from.</param>
        /// <param name="offset">The position withing <paramref name="buffer"/> to start at.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count) => o.Write(buffer, offset, count);

        /// <summary>
        /// Releases all resources of the stream, and disposes the input and output streams.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            i.Dispose();
            o.Dispose();
        }
    }
}