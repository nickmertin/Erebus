using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Vex472.Erebus.Client.Windows.DataModel;
using Vex472.Erebus.Core.Protocols;

namespace Vex472.Erebus.Client.Windows.Chat
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        static string[] img_ext = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".ico" };
        ObservableCollection<Message> messages = new ObservableCollection<Message>();
        SSMP chat;

        public ChatWindow(SSMP ssmp)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\472\Erebus\Client\Recent", ssmp.Endpoint.Value.Address.ToString(), DateTime.Now.Ticks);
            App.UsersTSM.RunSafe(() => App.RecentUpdate = true);
            InitializeComponent();
            DataContext = this;
            Title = "Secure chat with " + ssmp.Endpoint.Value.Address.ToString();
            chat = ssmp;
            list.ItemsSource = messages;
            chat.MessageReceived += (sender, e) =>
            {
                dynamic ad = null;
                if (e.HasAttachment)
                {
                    var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(path);
                    File.WriteAllBytes(path += "\\" + e.Attachment.Item1, e.Attachment.Item2);
                    ad = Tuple.Create(path, e.Attachment.Item1, img_ext.Contains(Path.GetExtension(e.Attachment.Item1).ToLower()));
                }
                Dispatcher.Invoke(() => messages.Add(new Message { Text = e.Message, Received = true, AttachmentData = ad, Timestamp = DateTime.Now }));
            };
            chat.ConnectionClosed += (sender, e) => Dispatcher.Invoke(Close);
        }

        private void key(object sender, KeyEventArgs e)
        {
            var t = text.Text;
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(t))
             {
                text.Text = "";
                if (Tag == null)
                    Task.Run(() =>
                    {
                        chat.SendMessage(t);
                        Dispatcher.Invoke(() => messages.Add(new Message { Text = t, Timestamp = DateTime.Now }));
                    });
                else
                {
                    var p = Tag as Tuple<string, string, bool>;
                    Tag = null;
                    Task.Run(() =>
                    {
                        chat.SendMessage(t, p.Item2, File.ReadAllBytes(p.Item1));
                        Dispatcher.Invoke(() => messages.Add(new Message { Text = t, Timestamp = DateTime.Now, AttachmentData = p }));
                    });
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog { Title = "Choose file to upload", CheckFileExists = true };
            if (d.ShowDialog() ?? false)
                Tag = Tuple.Create(d.FileName, Path.GetFileName(d.FileName), img_ext.Contains(Path.GetExtension(d.FileName)));
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var p = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
                Tag = Tuple.Create(p, Path.GetFileName(p), img_ext.Contains(Path.GetExtension(p)));
            }
        }

        private void removeAttachment(object sender, RoutedEventArgs e) => Tag = null;
    }
}