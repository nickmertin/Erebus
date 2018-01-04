using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Vex472.Erebus.Core.Utilities;
using Vex472.Erebus.Windows.Bluetooth;

namespace Vex472.Erebus.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ChooseBluetoothDevice.xaml
    /// </summary>
    public partial class ChooseBluetoothDevice : PageFunction<object>
    {
        Task ut;
        bool process;

        public ChooseBluetoothDevice()
        {
            InitializeComponent();
        }

        async Task devicesUpdateProcess()
        {
            while (process)
            {
                var dev = BluetoothUtils.EnumerateDevices();
                Dispatcher.Invoke(() =>
                {
                    var c = list.SelectedItem;
                    list.ItemsSource = dev;
                    if (c != null && dev.Contains(c))
                        list.SelectedIndex = dev.FirstIndexOf(c);
                });
                await Task.Delay(2000);
            }
        }

        private void load(object sender, RoutedEventArgs e)
        {
            process = true;
            ut = Task.Run(devicesUpdateProcess);
        }

        private void unload(object sender, RoutedEventArgs e)
        {
            process = false;
            ut.Wait();
            ut = null;
        }

        private void done(object sender, RoutedEventArgs e)
        {
            OnReturn(new ReturnEventArgs<object>(list.SelectedItem));
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            OnReturn(new ReturnEventArgs<object>(null));
        }
    }
}