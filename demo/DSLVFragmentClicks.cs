using System;

using Android.OS;
using Android.Widget;

namespace SampleDSLV
{
    public class DSLVFragmentClicks : DSLVFragment
    {

        public new static DSLVFragmentClicks newInstance(int headers, int footers)
        {
            DSLVFragmentClicks f = new DSLVFragmentClicks();

            Bundle args = new Bundle();
            args.PutInt("headers", headers);
            args.PutInt("footers", footers);
            f.Arguments = args;

            return f;
        }

        public override void OnActivityCreated(Bundle savedState)
        {
            base.OnActivityCreated(savedState);

            ListView lv = ListView;
            lv.ItemClick += (sender, e) =>
            {
                String message = String.Format("Clicked item {0}", e.Position);
                Toast.MakeText(Activity, message, ToastLength.Short).Show();

            };
            lv.ItemLongClick += (sender, e) =>
            {
                String message = String.Format("Long-clicked item {0}", e.Position);
                Toast.MakeText(Activity, message, ToastLength.Short).Show();
            };
        }
    }
}