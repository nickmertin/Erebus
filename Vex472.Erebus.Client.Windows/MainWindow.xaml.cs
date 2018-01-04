using System;
using System.Windows.Navigation;

namespace Vex472.Erebus.Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow() : this(new Uri("/Views/PasswordEntry.xaml", UriKind.Relative)) { }

        public MainWindow(Uri source)
        {
            InitializeComponent();
            Source = source;
        }

        private void navCanExec(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
    }
}