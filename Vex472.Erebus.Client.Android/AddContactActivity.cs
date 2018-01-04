using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Vex472.Erebus.Android.DataModel;
using Vex472.Erebus.Core.Utilities;
using Vex472.Erebus.Core;

namespace Vex472.Erebus.Client.Android
{
    [Activity(Label = "Add Contact")]
    public class AddContactActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var l = new LinearLayout(BaseContext) { Orientation = Orientation.Vertical };
            l.AddView(new TextView(BaseContext) { Text = "Please enter a name for the new contact:" });
            var t = new EditText(BaseContext);
            l.AddView(t);
            var b = new Button(BaseContext) { Text = "OK" };
            b.Click += (sender, e) =>
            {
                Configuration.TSM.RunSafe(() =>
                {
                    Configuration.Contacts = Configuration.Contacts.Append(new Contact(t.Text, new ErebusAddress(bundle.GetByteArray("address"))));
                    Configuration.Save(Common.ConfigurationPin);
                });
                Finish();
            };
        }
    }
}