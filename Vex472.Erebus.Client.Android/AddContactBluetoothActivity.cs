using Android.App;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;
using Vex472.Erebus.Android.Bluetooth;

namespace Vex472.Erebus.Client.Android
{
    [Activity(Label = "Select a Bluetooth device")]
    public class AddContactBluetoothActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var l = new LinearLayout(BaseContext) { Orientation = Orientation.Vertical };
            foreach (var d in BluetoothUtils.EnumerateDevices())
            {
                var b = new Button(BaseContext) { Text = $"{d.Name} ({d.Address})" };
                b.Click += (sender, e) => Task.Run(() => BluetoothUtils.RequestAddContact(d, Common.Instance.Address));
                l.AddView(b);
            }
            SetContentView(l);
        }
    }
}