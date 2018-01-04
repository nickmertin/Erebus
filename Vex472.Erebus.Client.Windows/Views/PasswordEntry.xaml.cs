using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Vex472.Erebus.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for PinEntry.xaml
    /// </summary>
    public partial class PasswordEntry : Page
    {
        public PasswordEntry()
        {
            InitializeComponent();
        }

        private void key(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var pass = (sender as PasswordBox).Password;
                var ns = NavigationService;
                Task.Run(() =>
                {
                    App.Initialize(pass);
                    Dispatcher.Invoke(() => ns.Navigate(new Uri("/Views/Home.xaml", UriKind.Relative)));
                });
                NavigationService.Navigate(new Uri("/Views/Loading.xaml", UriKind.Relative));
            }
        }
    }
}