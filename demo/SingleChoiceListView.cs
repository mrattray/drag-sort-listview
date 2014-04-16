using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using DragSortListview;
using DragSortListview.Helpers;
using DragSortListview.Interfaces;

namespace SampleDSLV
{
    [Activity(Label = "SingleChoiceListView")]
    public class SingleChoiceListView : ListActivity
    {
        private ArrayAdapter<String> adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.checkable_main);

            String[] array = Resources.GetStringArray(Resource.Array.jazz_artist_names);

            adapter = new ArrayAdapter<String>(this, Resource.Layout.list_item_radio, Resource.Id.text, new JavaList<String>(array));

            ListAdapter = adapter;

            DragSortListView list = ListView;
            list.SetDropListener(new DropListener((int from, int to) =>
            {
                if (from != to)
                {
                    String item = adapter.GetItem(from);
                    adapter.Remove(item);
                    adapter.Insert(item, to);
                    list.moveCheckState(from, to);
                    Log.Debug("DSLV", "Selected item is " + list.CheckedItemPosition);
                }
            }));
            list.ChoiceMode = ChoiceMode.Single;
        }

        public new DragSortListView ListView
        {
            get
            {
                return (DragSortListView)base.ListView;
            }
        }
    }
}