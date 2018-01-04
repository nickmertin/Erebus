using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;
using Java.IO;
using System;

namespace Vex472.Erebus.Android
{
    /// <summary>
    /// Provides code that initializes the Erebus libraries.
    /// </summary>
    public static class Initialization
    {
        static ThreadSafetyManager logFileTsm = new ThreadSafetyManager();
        static File logFile = new File(global::Android.OS.Environment.ExternalStorageDirectory, "Erebus.log");

        /// <summary>
        /// Initializes the Erebus libraries.
        /// </summary>
        public static void Run()
        {
            HDP.Register();
            SSMP.Register();
            CNP.Register();
            PlatformServiceProvider.RegisterService<VerificationKeyProvider>();
            Log.EntryRecorded += (sender, e) => logFileTsm.RunSafe(() =>
            {
                using (var w = new FileWriter(logFile, true))
                using (var p = new PrintWriter(w))
                {
                    p.Println($"[{DateTime.Now}] [{sender}/{e.Severity}]: {e.Message}");
                    p.Flush();
                }
            });
            Log.EntryRecorded += (sender, e) =>
            {
                switch (e.Severity)
                {
                    case LogEntrySeverity.Info:
                        global::Android.Util.Log.Info(sender.ToString(), e.Message);
                        break;
                    case LogEntrySeverity.Warning:
                        global::Android.Util.Log.Warn(sender.ToString(), e.Message);
                        break;
                    case LogEntrySeverity.Error:
                        global::Android.Util.Log.Error(sender.ToString(), e.Message);
                        break;
                    case LogEntrySeverity.Fatal:
                        global::Android.Util.Log.Error(sender.ToString(), "FATAL: " + e.Message);
                        break;
                }
            };
        }
    }
}