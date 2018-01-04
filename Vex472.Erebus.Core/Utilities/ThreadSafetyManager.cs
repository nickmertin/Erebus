using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Wraps a <see cref="Mutex"/> and provides methods to ensure thread-safe execution of a routine.
    /// </summary>
    public sealed class ThreadSafetyManager : IDisposable
    {
        Mutex mtx = new Mutex();

        /// <summary>
        /// Runs the specified routine synchronously in a thread-safe manner.
        /// </summary>
        /// <param name="operation">The routine to run.</param>
        public void RunSafe(Action operation)
        {
            mtx.WaitOne();
            try { operation(); }
            finally { mtx.ReleaseMutex(); }
        }

        /// <summary>
        /// Runs the specified routine asynchronously in a thread-safe manner.
        /// </summary>
        /// <param name="operation">The routine to run</param>
        /// <returns>A handle to the task that is executing the routine.</returns>
        public Task RunSafeAsync(Action operation) => Task.Run(() => RunSafe(operation));

        /// <summary>
        /// Runs the specified routine synchronously in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="operation">The routine to run.</param>
        /// <returns>The result of the routine.</returns>
        public T RunSafe<T>(Func<T> operation)
        {
            mtx.WaitOne();
            T result;
            try { result = operation(); }
            finally { mtx.ReleaseMutex(); }
            return result;
        }

        /// <summary>
        /// Runs the specified routine asynchronously in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="operation">The routine to run.</param>
        /// <returns>A handle to the task that is executing the routine, to retrieve the result.</returns>
        public Task<T> RunSafeAsync<T>(Func<T> operation) => Task.Run(() => RunSafe(operation));

        /// <summary>
        /// Releases the internal mutex.
        /// </summary>
        public void Dispose() => mtx.Dispose();
    }
}