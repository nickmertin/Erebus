using System;
using System.Linq;
using System.Windows.Forms;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;
using Vex472.Erebus.Windows;
using Vex472.Erebus.Windows.TCPIP;

namespace Vex472.Erebus.Server
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Log.EntryRecorded += (sender, e) => Console.WriteLine($"{sender} ({e.Severity}): {e.Message}");
            VerificationKeyProvider.Initialize("keyinfo.dat", "hello world");
            var vkp = new VerificationKeyProvider();
            Initialization.Run();
            SSMP ssmp = null;
            SSMP.InvitationReceived += (sender, e) =>
            {
                if (ssmp == null)
                    ssmp = e.Accept();
                ssmp.MessageReceived += Ssmp_MessageReceived;
                ssmp.ConnectionClosed += (_sender, _e) => ssmp = null;
            };
            var ei = new ErebusInstance(new ErebusAddress(Console.ReadLine()));
            ei.Services = new[] { HDPService.SSMP };
            var l = new TCPIPConnectionListener(int.Parse(Console.ReadLine()), ei);
            while (true)
                try
                {
                    var cmd = Console.ReadLine().Split(' ');
                    switch (cmd[0])
                    {
                        case "addlink":
                            new ErebusLink(TCPIPConnectionUtils.Connect(cmd[1], int.Parse(cmd[2])), ei, false);
                            break;
                        case "listlink":
                            foreach (var link in ei.Links)
                                Console.WriteLine(link.RemoteAddress);
                            break;
                        case "hdp":
                            var hdp = new HDP(ei, HDPService.SSMP);
                            hdp.SendRequest(ErebusAddress.Broadcast);
                            hdp.InitClose();
                            break;
                        case "ssmp":
                            ssmp = new SSMP(ei, new ErebusAddress(cmd[1]));
                            ssmp.MessageReceived += Ssmp_MessageReceived;
                            break;
                        case "msg":
                            ssmp?.SendMessage(cmd.Skip(1).Aggregate((x, y) => x + " " + y));
                            break;
                        case "addkey":
                            var addr = new ErebusAddress(cmd[1]);
                            vkp.RemoveKeyPair(addr);
                            var key = vkp.CreatePrivateKey();
                            Console.WriteLine("Key: " + key.Item2.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y));
                            Console.Write("Please copy other key to clipboard, then press <enter> . . .");
                            Console.ReadLine();
                            vkp.AddKeyPair(addr, key.Item1, Clipboard.GetText().Split(',').Select(x => byte.Parse(x)).ToArray());
                            break;
                        case "removekey":
                            vkp.RemoveKeyPair(new ErebusAddress(cmd[1]));
                            break;
                        case "close":
                            ssmp?.Dispose();
                            ssmp = null;
                            break;
                        case "exit":
                            return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        static void Ssmp_MessageReceived(object sender, SSMPMessageReceivedEventArgs e) => Console.WriteLine($"Message received{(e.HasAttachment ? " (with attachment)" : "")}: {e.Message}");
    }
}