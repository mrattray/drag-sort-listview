using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using DragSortListview;
using DragSortListview.Helpers;

namespace SampleDSLV
{
    [Activity(Label = "MultipleChoiceListView")]
    public class MultipleChoiceListView : ListActivity
    {
        private ArrayAdapter<String> adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.checkable_main);

            String[] array = Resources.GetStringArray(Resource.Array.jazz_artist_names);
            JavaList<String> jlist = new JavaList<String>(array);

            adapter = new ArrayAdapter<String>(this, Resource.Layout.list_item_checkable, Resource.Id.text, jlist);

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
                }
            }));
            list.SetRemoveListener(new RemoveListener((int which) =>
            {
                String item = adapter.GetItem(which);
                adapter.Remove(item);
                list.removeCheckState(which);
            }));
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