using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DragSortListview;

namespace SampleDSLV
{
    [Activity(Label = "CursorDSLV")]
    public class CursorDSLV : FragmentActivity
    {
        private SimpleDragSortCursorAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.cursor_main);

            String[] cols = { "name" };
            int[] ids = { Resource.Id.text };
            adapter = new MAdapter(this,
                    Resource.Layout.list_item_click_remove, null, cols, ids, 0);

            DragSortListView dslv = (DragSortListView)FindViewById(Android.Resource.Id.List);
            dslv.Adapter = adapter;

            // build a cursor from the String array
            MatrixCursor cursor = new MatrixCursor(new String[] { "_id", "name" });
            String[] artistNames = Resources.GetStringArray(Resource.Array.jazz_artist_names);
            for (int i = 0; i < artistNames.Length; i++)
            {
                cursor.NewRow()
                        .Add(i)
                        .Add(artistNames[i]);
            }
            adapter.ChangeCursor(cursor);
        }

        private class MAdapter : SimpleDragSortCursorAdapter
        {
            private Context mContext;

            public MAdapter(Context ctxt, int rmid, ICursor c, String[] cols, int[] ids, int something)
                : base(ctxt, rmid, c, cols, ids, something)
            {
                mContext = ctxt;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View v = base.GetView(position, convertView, parent);
                View tv = v.FindViewById(Resource.Id.text);
                tv.Click += (s, e) =>
                {
                    Toast.MakeText(mContext, "text clicked", ToastLength.Short).Show();
                };

                return v;
            }
        }
    }
}