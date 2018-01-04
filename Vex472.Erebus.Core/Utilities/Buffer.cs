using System;
using System.Collections.Generic;
using System.Linq;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Represents a dynamically sized data buffer
    /// </summary>
    public sealed class Buffer
    {
        const int elemSize = 1024;
        LinkedList<byte[]> data = new LinkedList<byte[]>();
        int lastSize = 1024;
        
        /// <summary>
        /// Creates a new, empty instance of the <see cref="Buffer"/> class.
        /// </summary>
        public Buffer() { }

        /// <summary>
        /// Retrieves the current contents of the <see cref="Buffer"/> as a single byte array.
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] GetAsByteArray() => data.HasElements() ? data.Range(0, data.Count - 1).SelectMany(x => x).Concat(data.Last.Value.Range(0, lastSize)).ToArray() : new byte[0];

        /// <summary>
        /// Empties the <see cref="Buffer"/>
        /// </summary>
        public void Clear()
        {
            data.Clear();
            lastSize = 1024;
        }

        /// <summary>
        /// Appends the given byte array to the end of the <see cref="Buffer"/>.
        /// </summary>
        /// <param name="array">The byte array to append.</param>
        public void Append(byte[] array)
        {
            if (array.Length == 0)
                return;
            if (lastSize == elemSize)
            {
                data.AddLast(new byte[elemSize]);
                lastSize = 0;
            }
            if (lastSize + array.Length <= elemSize)
            {
                Array.Copy(array, 0, data.Last.Value, lastSize, array.Length);
                lastSize += array.Length;
            }
            else
            {
                int count = elemSize - lastSize;
                Array.Copy(array, 0, data.Last.Value, lastSize, count);
                lastSize = elemSize;
                Append(array.Skip(count).ToArray());
            }
        }

        /// <summary>
        /// Gets the current capacity of the <see cref="Buffer"/>.
        /// </summary>
        public long Capacity => data.Count * elemSize;

        /// <summary>
        /// Gets the current number of bytes in the <see cref="Buffer"/>.
        /// </summary>
        public long Size => Capacity - elemSize + lastSize;
    }
}