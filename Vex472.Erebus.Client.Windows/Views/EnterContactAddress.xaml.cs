using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Protocols;

namespace Vex472.Erebus.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for EnterContactAddress.xaml
    /// </summary>
    public partial class EnterContactAddress : Page
    {
        static bool done;

        public EnterContactAddress()
        {
            InitializeComponent();
        }

        private void key(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ErebusAddress addr;
                try
                {
                    addr = new ErebusAddress(address.Text);
                }
                catch
                {
                    MessageBox.Show(Application.Current.MainWindow, "The address that you have entered appears to be in an invalid format.", "Erebus");
                    return;
                }
                var ns = NavigationService;
                var d = Dispatcher;
                done = true;
                ns.Navigate(new Uri("/Views/Loading.xaml", UriKind.Relative));
                Task.Run(() => d.Invoke(new CNP(App.Instance).RequestContact(addr, pin => d.Invoke(() => MessageBox.Show(Application.Current.MainWindow, $"Please verify that the following PIN matches the one displayed by {addr}: {pin}", "Add Contact", MessageBoxButton.YesNo)) == MessageBoxResult.Yes) ? new Action(() =>
                        {
                            ns.GoBack();
                            App.AddContactData = new { Address = addr, Name = addr.ToString() };
                            ns.Navigated += Ns_Navigated;
                        }) : ns.GoBack));
            }
        }

        private void Ns_Navigated(object sender, NavigationEventArgs e)
        {
            var ns = (sender as MainWindow).NavigationService;
            ns.Navigated -= Ns_Navigated;
            ns.Navigate(new Uri("/Views/AddContact.xaml", UriKind.Relative));
        }

        private void load(object sender, RoutedEventArgs e)
        {
            if (done)
            {
                NavigationService.GoBack();
                done = false;
            }
        }
    }
}