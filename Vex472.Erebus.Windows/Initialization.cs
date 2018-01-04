using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Windows
{
    /// <summary>
    /// Provides code that initializes the Erebus libraries.
    /// </summary>
    public static class Initialization
    {
        /// <summary>
        /// Initializes the Erebus libraries.
        /// </summary>
        public static void Run()
        {
            HDP.Register();
            SSMP.Register();
            CNP.Register();
            PlatformServiceProvider.RegisterService<VerificationKeyProvider>();
        }
    }
}