using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Vex472.Erebus.Client.Windows.Chat;
using Vex472.Erebus.Client.Windows.DataModel;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Windows;
using Vex472.Erebus.Windows.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;

namespace Vex472.Erebus.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        UserFilter filter = UserFilter.All;

        public Home()
        {
            InitializeComponent();
            address.Text = "My address: " + App.Instance.Address.ToString();
            SSMP.InvitationReceived += (sender, e) =>
            {
                using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client\Contacts"))
                    if (Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow, $"{k.GetValue(e.Source.ToString())} has invited you to a conversation, would you like to accept?", "Incoming Invitation - Erebus", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                        Dispatcher.Invoke(() => new ChatWindow(e.Accept()).Show());
            };
            CNP.RequestReceived += (sender, e) =>
            {
                if (Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow, $"{e.Address} has sent you a contact request, would you like to accept? The connection PIN is {e.PIN}.", "Incoming Contact Request - Erebus", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                {
                    e.Accept();
                    App.AddContactData = new { Address = e.Address, Name = e.Address.ToString() };
                    Dispatcher.Invoke(() => NavigationService.Navigate(new Uri("/Views/AddContact.xaml", UriKind.Relative)));
                }
            };
        }

        private void all(object sender, RoutedEventArgs e)
        {
            filter = UserFilter.All;
            App.ForceUsersUpdate();
        }

        private void online(object sender, RoutedEventArgs e)
        {
            filter = UserFilter.Online;
            App.ForceUsersUpdate();
        }

        private void recent(object sender, RoutedEventArgs e)
        {
            filter = UserFilter.Recent;
            App.ForceUsersUpdate();
        }

        private void load(object sender, RoutedEventArgs e)
        {
            App.UserListUpdated += App_UserListUpdated;
            App.ForceUsersUpdate();
        }

        private void unload(object sender, RoutedEventArgs e) => App.UserListUpdated -= App_UserListUpdated;

        private void App_UserListUpdated(object sender, UserListUpdatedEventArgs e) => Dispatcher.Invoke(() =>
            {
                switch (filter)
                {
                    case UserFilter.All:
                        list.ItemsSource = e.All;
                        break;
                    case UserFilter.Online:
                        list.ItemsSource = e.Online;
                        break;
                    case UserFilter.Recent:
                        list.ItemsSource = e.Recent;
                        break;
                }
                list.Items.Refresh();
            });

        private void addContactBt(object sender, RoutedEventArgs e)
        {
            var page = new ChooseBluetoothDevice();
            page.Return += ChooseBluetoothDevice_Return;
            NavigationService.Navigate(page);
        }

        private void addContactNet(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Uri("/Views/EnterContactAddress.xaml", UriKind.Relative));

        private void ChooseBluetoothDevice_Return(object sender, ReturnEventArgs<object> e) => Task.Run(() =>
        {
            var svc = e.Result as RfcommDeviceService;
            if (svc == null)
                return;
            var addr = BluetoothUtils.RequestAddContact(svc, App.Instance.Address);
            App.AddContactData = new { Address = addr.Value, Name = $"Bluetooth device '{svc.ConnectionHostName.DisplayName}'" };
            Dispatcher.Invoke(addr == null ? new Action(() => MessageBox.Show(Application.Current.MainWindow, $"'{svc.ConnectionHostName.DisplayName}' rejected your contact request.")) : new Action(() => NavigationService.Navigate(new Uri("/Views/AddContact.xaml", UriKind.Relative))));
        });

        private void removeContact(object sender, RoutedEventArgs e)
        {
            var user = list.SelectedItem as User;
            if (MessageBox.Show(Application.Current.MainWindow, $"Are you sure you want to remove {user.Name} from your contacts?", "Remove Contact - Erebus", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                Task.Run(() =>
                {
                    App.UsersTSM.RunSafe(() =>
                    {
                        using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client\Contacts"))
                            k.DeleteValue(user.Address.ToString());
                        using (var k = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\472\Erebus\Client\Recent"))
                            if (k.GetValueNames().Contains(user.Address.ToString()))
                                k.DeleteValue(user.Address.ToString());
                        App.MainUpdate = App.RecentUpdate = true;
                    });
                    new VerificationKeyProvider().RemoveKeyPair(user.Address);
                });
        }

        private void chat(object sender, RoutedEventArgs e) => Task.Run(() =>
            {
                var ssmp = new SSMP(App.Instance, Dispatcher.Invoke(() => (list.SelectedItem as User).Address));
                Dispatcher.Invoke(() => new ChatWindow(ssmp).Show());
            });

        private void copyAddress(object sender, RoutedEventArgs e) => Clipboard.SetText(App.Instance.Address.ToString());
    }
}