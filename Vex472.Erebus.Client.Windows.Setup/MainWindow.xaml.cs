using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using Vex472.Erebus.Core;
using Vex472.Erebus.Windows;

namespace Vex472.Erebus.Client.Windows.Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ErebusAddress addr;
        Random r = new Random();
        
        public MainWindow()
        {
            InitializeComponent();
            regenAddr();
            path.Text = Path.Combine(Environment.GetEnvironmentVariable("localappdata"), "Erebus\\Client");
        }

        void regenAddr()
        {
            var b = new byte[16];
            r.NextBytes(b);
            addr = new ErebusAddress(b);
            address.Text = addr.ToString();
        }

        private void regenerateAddress(object sender, RoutedEventArgs e) => regenAddr();

        private void done(object sender, RoutedEventArgs e)
        {
            using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client"))
            {
                k.SetValue("logfile", Path.Combine(path.Text, "Erebus.log"));
                k.SetValue("keyfile", Path.Combine(path.Text, "keyinfo.dat"));
                k.SetValue("address", addr.ToString());
                using (var tk = k.CreateSubKey("Targets"))
                    tk.SetValue("mertin1", 1234);
            }
            Directory.CreateDirectory(path.Text);
            VerificationKeyProvider.Initialize(Path.Combine(path.Text, "keyinfo.dat"), password.Password);
            Close();
        }
    }
}