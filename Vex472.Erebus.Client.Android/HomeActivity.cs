using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Linq;
using System.Threading;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core.Protocols;
using Vex472.Erebus.Core.Utilities;

namespace Vex472.Erebus.Client.Android
{
    [Activity(Label = "Home")]
    public class HomeActivity : Activity
    {
        ContactAdapter a;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            /*CNP.RequestReceived += (sender, e) =>
            {
                using (var ewh = new EventWaitHandle(false, EventResetMode.ManualReset))
                {
                    var go = false;
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            var b = new AlertDialog.Builder(BaseContext);
                            b.SetCancelable(true);
                            b.SetIcon(Resource.Drawable.Icon);
                            b.SetMessage($"{e.Address} has sent you a contact request, would you like to accept? The connection PIN is {e.PIN}.");
                            b.SetTitle("Incoming Contact Request");
                            b.SetPositiveButton("Yes", (_sender, _e) =>
                            {
                                go = true;
                                ewh.Set();
                                var _b = new Bundle(1);
                                _b.PutByteArray("address", e.Address.Serialize());
                                StartActivity(typeof(AddContactActivity));
                            });
                            b.SetNegativeButton("No", (_sender, _e) => ewh.Set());
                            b.Create().Show();
                        }
                        catch (Exception ex)
                        {
                            Log.RecordEvent(this, $"handler failed with message {ex.Message}", LogEntrySeverity.Error);
                        }
                    });
                    ewh.WaitOne();
                    if (go)
                        e.Accept();
                }
            };*/
            CNP.RequestReceived += (sender, e) => e.Accept();
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            a = new ContactAdapter(Configuration.Contacts);
            var list = new ListView(BaseContext) { Adapter = a };
            SetContentView(list);
            var all = ActionBar.NewTab();
            all.SetText("All");
            all.TabSelected += All_TabSelected;
            all.TabReselected += All_TabSelected;
            ActionBar.AddTab(all);
            var online = ActionBar.NewTab();
            online.SetText("Online");
            online.TabSelected += All_TabSelected;
            online.TabReselected += All_TabSelected;
            ActionBar.AddTab(online);
            var recent = ActionBar.NewTab();
            recent.SetText("Recent");
            recent.TabSelected += Recent_TabSelected;
            recent.TabReselected += Recent_TabSelected;
            ActionBar.AddTab(recent);
            ActionBar.SetLogo(Resource.Drawable.Icon);
        }

        private void All_TabSelected(object sender, ActionBar.TabEventArgs e) => a.Array = (from c in Configuration.Contacts orderby c.Name.ToLower() ascending select c).ToArray();

        private void Recent_TabSelected(object sender, ActionBar.TabEventArgs e) => a.Array = (from c in Configuration.Contacts join r in Configuration.Recents on c.Address equals r.Address orderby r.Timestamp descending select c).ToArray();

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var s = menu.AddSubMenu("Add Contact");
            s.Add("Bluetooth").SetIntent(new Intent(BaseContext, typeof(AddContactBluetoothActivity)));
            return true;
        }

        class ContactAdapter : BaseAdapter<Contact>
        {
            public ContactAdapter(Contact[] array)
            {
                Array = array;
            }

            public Contact[] Array { get; set; }

            public override Contact this[int position] => Array[position];

            public override int Count => Array.Length;

            public override long GetItemId(int position) => position;

            public override View GetView(int position, View convertView, ViewGroup parent) => new TextView(parent.Context) { Text = Array[position].Name };
        }
    }
}