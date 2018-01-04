using Microsoft.Win32;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Vex472.Erebus.Core;

namespace Vex472.Erebus.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for AddContact.xaml
    /// </summary>
    public partial class AddContact : Page
    {
        ErebusAddress addr;

        public AddContact()
        {
            InitializeComponent();
            addr = App.AddContactData.Address;
            text.Text = $"Please enter a name for your new contact ({App.AddContactData.Name}):";
        }

        private void key(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\472\Erebus\Client\Contacts", addr.ToString(), name.Text);
                NavigationService.GoBack();
                App.UsersTSM.RunSafe(() => App.MainUpdate = true);
            }
        }
    }
}