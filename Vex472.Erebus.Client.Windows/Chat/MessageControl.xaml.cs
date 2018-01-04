using System.Diagnostics;
using System.Windows.Controls;
using Vex472.Erebus.Client.Windows.DataModel;

namespace Vex472.Erebus.Client.Windows.Chat
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public MessageControl()
        {
            InitializeComponent();
        }

        private void open(object sender, System.Windows.RoutedEventArgs e)
        {
            dynamic d = DataContext;
            Process.Start((DataContext as Message).AttachmentData.Item1);
        }
    }
}