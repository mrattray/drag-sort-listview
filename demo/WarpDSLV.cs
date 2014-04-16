using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using DragSortListview;
using DragSortListview.Helpers;

namespace SampleDSLV
{
    [Activity(Label = "WarpDSLV")]
    public class WarpDSLV : ListActivity
    {
        private ArrayAdapter<String> adapter;

        private String[] array;
     
        /** Called when the activity is first created. */
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.warp_main);

            DragSortListView lv = (DragSortListView)ListView;

            array = Resources.GetStringArray(Resource.Array.countries);

            adapter = new ArrayAdapter<String>(this, Resource.Layout.list_item_handle_right, Resource.Id.text, new JavaList<String>(array));
            ListAdapter = adapter;

            lv.setDragScrollProfile(new MyDragScrollProfile(adapter));

            lv.SetDropListener(new DropListener((int from, int to) =>
            {
                String item = adapter.GetItem(from);

                adapter.NotifyDataSetChanged();
                adapter.Remove(item);
                adapter.Insert(item, to);
            }));
            lv.SetRemoveListener(new RemoveListener((int which) =>
            {
                adapter.Remove(adapter.GetItem(which));
            }));
        }

        private class MyDragScrollProfile : DragSortListview.DragSortListView.DragScrollProfile
        {
            ArrayAdapter mAdapter;
            public MyDragScrollProfile(ArrayAdapter adapter)
            {
                mAdapter = adapter;
            }

            public override float GetSpeed(float w, long t)
            {
                if (w > 0.8f)
                {
                    // Traverse all views in a millisecond
                    return ((float)mAdapter.Count) / 0.001f;
                }
                else
                {
                    return 10.0f * w;
                }
            }
        }
    }
}