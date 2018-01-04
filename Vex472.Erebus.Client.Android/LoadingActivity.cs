using Android.App;
using Android.OS;
using Android.Widget;

namespace Vex472.Erebus.Client.Android
{
    [Activity(Label = "Loading...")]
    public class LoadingActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(new ProgressBar(BaseContext) { Indeterminate = true });
        }
    }
}