using Android.App;
using Android.OS;
using Android.Support.V4.App;

namespace SampleDSLV
{
    [Activity(Label = "BGHandle")]
    public class BGHandle : FragmentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.bg_handle_main);
        }
    }
}