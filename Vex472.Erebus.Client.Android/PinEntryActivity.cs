using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using Vex472.Erebus.Android;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Android.TCPIP;
using Vex472.Erebus.Core;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Client.Android
{
    [Activity(Label = "Erebus", MainLauncher = true, Icon = "@drawable/Icon")]
    public class PinEntryActivity : Activity
    {
        int pin = 0;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                Initialization.Run();
                var baseLayout = new LinearLayout(BaseContext) { Orientation = Orientation.Vertical };
                baseLayout.AddView(new TextView(BaseContext) { Text = "Please enter your PIN:", Gravity = GravityFlags.CenterHorizontal, Top = 15 });
                var currentPin = new TextView(BaseContext) { Gravity = GravityFlags.CenterHorizontal, Top = 10 };
                baseLayout.AddView(currentPin);
                var grid = new GridLayout(BaseContext) { ColumnCount = 3, RowCount = 4, HorizontalScrollBarEnabled = false, VerticalScrollBarEnabled = false };
                foreach (var c in new[] { '7', '8', '9', '4', '5', '6', '1', '2', '3', 'X', '0', '>' })
                {
                    var btn = new Button(BaseContext) { Text = c.ToString(), TextSize = 24 };
                    btn.Click += (sender, e) =>
                    {
                        switch ((sender as Button).Text)
                        {
                            case "0":
                                pin *= 10;
                                break;
                            case "1":
                                pin *= 10;
                                pin += 1;
                                break;
                            case "2":
                                pin *= 10;
                                pin += 2;
                                break;
                            case "3":
                                pin *= 10;
                                pin += 3;
                                break;
                            case "4":
                                pin *= 10;
                                pin += 4;
                                break;
                            case "5":
                                pin *= 10;
                                pin += 5;
                                break;
                            case "6":
                                pin *= 10;
                                pin += 6;
                                break;
                            case "7":
                                pin *= 10;
                                pin += 7;
                                break;
                            case "8":
                                pin *= 10;
                                pin += 8;
                                break;
                            case "9":
                                pin *= 10;
                                pin += 9;
                                break;
                            case "X":
                                if (pin > 0)
                                    pin /= 10;
                                break;
                            case ">":
                                Task.Run(() =>
                                {
                                    if (Configuration.IsConfigured)
                                    {
                                        try
                                        {
                                            Configuration.Reload(pin);
                                        }
                                        catch (Exception _e)
                                        {
                                            Log.RecordEvent(this, $"Error while reloading configuration: {_e.Message}", LogEntrySeverity.Error);
                                            pin = 0;
                                            RunOnUiThread(() => currentPin.Text = "");
                                            return;
                                        }
                                        RunOnUiThread(() => StartActivity(typeof(LoadingActivity)));
                                    }
                                    else
                                    {
                                        RunOnUiThread(() => StartActivity(typeof(LoadingActivity)));
                                        Configuration.Reset(new ErebusAddress("{472--c}"));
                                        Configuration.TSM.RunSafe(() => Configuration.ConnectionTargets = new IConnectionTarget[] { new TCPIPConnectionTarget("192.168.42.80", 1234) });
                                        Configuration.Save(pin);
                                    }
                                    Common.Instance = new ErebusInstance(Configuration.Address) { Services = new[] { HDPService.SSMP } };
                                    foreach (var t in Configuration.ConnectionTargets)
                                        new ErebusLink(t.Connect(), Common.Instance, false);
                                    RunOnUiThread(() => StartActivity(typeof(HomeActivity)));
                                    Common.ConfigurationPin = pin;
                                });
                                return;
                        }
                        currentPin.Text = pin > 0 ? new string('*', (int)Math.Floor(Math.Log10(pin)) + 1) : "";
                    };
                    grid.AddView(btn);
                }
                baseLayout.AddView(grid);
                SetContentView(baseLayout);
            }
            catch (Exception e)
            {
                Log.RecordEvent(this, $"Exception while setting up PIN entry GUI: {e.Message}", LogEntrySeverity.Fatal);
                Finish();
            }
        }
    }
}