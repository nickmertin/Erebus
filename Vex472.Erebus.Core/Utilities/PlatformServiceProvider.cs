using System;
using System.Collections.Generic;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Exposes platform-specific service implementations to cross-platform code.
    /// </summary>
    public static class PlatformServiceProvider
    {
        static Dictionary<string, Func<dynamic>> factories = new Dictionary<string, Func<dynamic>>();

        /// <summary>
        /// Registers a service implementation.
        /// </summary>
        /// <typeparam name="T">The implementation type.</typeparam>
        public static void RegisterService<T>() where T : new() => factories.Add(typeof(T).Name, () => new T());

        /// <summary>
        /// Creates an instance of the service with the specified name.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <returns></returns>
        public static dynamic Create(string name) => factories[name]();
    }
}