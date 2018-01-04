using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vex472.Erebus.Client.Windows.DataModel;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;
using Vex472.Erebus.Windows;
using Vex472.Erebus.Windows.Bluetooth;
using Vex472.Erebus.Windows.TCPIP;

namespace Vex472.Erebus.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string LogFile, KeyFile;
        public static ErebusInstance Instance;
        public static Tuple<string, int>[] ConnectionTargets;
        public static ThreadSafetyManager UsersTSM = new ThreadSafetyManager();
        public static bool MainUpdate = true, RecentUpdate = true;
        public static dynamic AddContactData;
        static List<EventHandler<UserListUpdatedEventArgs>> uh = new List<EventHandler<UserListUpdatedEventArgs>>();
        static bool process = true;
        static ErebusAddress[] _online = new ErebusAddress[0];
        static bool onlineUpdate = true, forceUpdate = true;
        static BluetoothConnectionListener btConnListener;
        static BluetoothContactRequestListener btReqListener;

        const int ONLINE_TIMEOUT = 5000;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool failed = false;
            while (true)
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\472\Erebus\Client"))
                    {
                        LogFile = k.GetValue("logfile") as string;
                        KeyFile = k.GetValue("keyfile") as string;
                        Log.EntryRecorded += (sender, _e) => File.AppendAllLines(LogFile, new[] { $"[{DateTime.Now}] [{sender}/{_e.Severity}]: {_e.Message}" });
#if DEBUG
                        Console.Title = "Erebus Client Log Window";
                        Log.EntryRecorded += (sender, _e) => Console.WriteLine($"[{DateTime.Now}] [{sender}/{_e.Severity}]: {_e.Message}");
#endif
                        Instance = new ErebusInstance(new ErebusAddress(k.GetValue("address") as string)) { Services = new[] { HDPService.SSMP } };
                        using (var ctk = k.OpenSubKey("Targets"))
                            ConnectionTargets = (from t in ctk.GetValueNames() select Tuple.Create(t, (int)ctk.GetValue(t))).ToArray();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (failed)
                    {
                        Log.RecordEvent(this, $"Application startup failed a second time: {ex.Message}; unable to recover.", LogEntrySeverity.Fatal);
                        throw new Exception("Application startup failed a second time.", ex);
                    }
                    Log.RecordEvent(this, $"Error during appliciation startup: {ex.Message}; starting setup utility.", LogEntrySeverity.Error);
                    Process.Start(Path.Combine(Environment.CurrentDirectory, "Vex472.Erebus.Client.Windows.Setup.exe")).WaitForExit();
                    failed = true;
                }
            Log.RecordEvent(this, "Application started.", LogEntrySeverity.Info);
        }

        public static void Initialize(string keyPass)
        {
            VerificationKeyProvider.Initialize(KeyFile, keyPass);
            Initialization.Run();
            foreach (var t in ConnectionTargets)
                try
                {
                    new ErebusLink(TCPIPConnectionUtils.Connect(t.Item1, t.Item2), Instance, false);
                }
                catch (Exception e)
                {
                    Log.RecordEvent(typeof(App), $"Failed to connect to {t.Item1}:{t.Item2}: {e.Message}", LogEntrySeverity.Error);
                }
            btConnListener = new BluetoothConnectionListener(Instance);
            btReqListener = new BluetoothContactRequestListener(Instance.Address, (addr, name) =>
                {
                    if (Current.Dispatcher.Invoke(() => MessageBox.Show(Current.MainWindow, "Would you like to accept this request?", $"Received contact request via Bluetooth from {name}", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                    {
                        Current.Dispatcher.Invoke(() => (Current.MainWindow as MainWindow).Navigate(new Uri("/Views/AddContact.xaml", UriKind.Relative), new { Address = addr, Name = $"Bluetooth device '{name}'" }));
                        return true;
                    }
                    return false;
                });
            Task.Run(usersUpdateProcess);
            Task.Run(onlineUpdateProcess);
        }

        public static void ForceUsersUpdate()
        {
            UsersTSM.RunSafe(() => forceUpdate = true);
            Log.RecordEvent(typeof(App), "Users list update forced.", LogEntrySeverity.Info);
        }

        static async Task usersUpdateProcess()
        {
            User[] users = null, online = null, recent = null;
            while (process)
            {
                bool update = false;
                if (UsersTSM.RunSafe(() =>
                {
                    var y = MainUpdate;
                    MainUpdate = false;
                    return y;
                }))
                {
                    using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client\Contacts"))
                        users = (from c in k.GetValueNames() select new User(k.GetValue(c) as string, ErebusAddress.Parse(c))).ToArray();
                    update = true;
                }
                if (UsersTSM.RunSafe(() =>
                {
                    var y = onlineUpdate;
                    onlineUpdate = false;
                    return y;
                }))
                {
                    online = (from c in users where _online.Contains(c.Address) select c).ToArray();
                    update = true;
                }
                if (UsersTSM.RunSafe(() =>
                {
                    var y = RecentUpdate;
                    RecentUpdate = false;
                    return y;
                }))
                {
                    using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client\Recent"))
                    {
                        var addresses = from a in k.GetValueNames() select new { Address = ErebusAddress.Parse(a), Text = a };
                        recent = (from c in users join a in addresses on c.Address equals a.Address orderby k.GetValue(a.Text) descending select c).ToArray();
                    }
                    update = true;
                }
                if (UsersTSM.RunSafe(() =>
                {
                    var y = forceUpdate;
                    forceUpdate = false;
                    return y;
                }) || update)
                {
                    foreach (var u in users)
                        u.Online = online.Contains(u);
                    lock (uh)
                        foreach (var h in uh)
                            h(null, new UserListUpdatedEventArgs(users, online, recent));
                    Log.RecordEvent(typeof(App), "Users list updated.", LogEntrySeverity.Info);
                }
                await Task.Delay(100);
            }
        }

        static async Task onlineUpdateProcess()
        {
            while (process)
            {
                var l = new List<ErebusAddress>();
                var hdp = new HDP(Instance, HDPService.SSMP);
                hdp.ResponseReceived += (sender, e) => { lock (l) l.Add(e.ServerAddress); };
                hdp.SendRequest(ErebusAddress.Broadcast);
                hdp.InitClose(ONLINE_TIMEOUT);
                await Task.Delay(ONLINE_TIMEOUT);
                UsersTSM.RunSafe(() =>
                {
                    _online = l.ToArray();
                    onlineUpdate = true;
                });
            }
        }

        public static event EventHandler<UserListUpdatedEventArgs> UserListUpdated
        {
            add { lock (uh) uh.Add(value); }
            remove { lock (uh) uh.Remove(value); }
        }
    }
}