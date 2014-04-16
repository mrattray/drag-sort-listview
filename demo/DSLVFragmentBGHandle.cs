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
using DragSortListview;

namespace SampleDSLV
{
    public class DSLVFragmentBGHandle : DSLVFragment
    {

        public new int getItemLayout()
        {
            return Resource.Layout.list_item_bg_handle;
        }

        public override IListAdapter ListAdapter
        {
            set
            {
                IListAdapter adapter = value;
                String[] array = Resources.GetStringArray(Resource.Array.jazz_artist_names);
                adapter = new MyAdapter(array, this);
                base.ListAdapter = adapter;
            }
            get
            {
                return base.ListAdapter;
            }
        }

        public override DragSortController BuildController(DragSortListView dslv)
        {
            MyDSController c = new MyDSController(dslv, this);
            return c;
        }


        private class MyAdapter : ArrayAdapter<String>
        {
            private Activity context;
            public MyAdapter(String[] artists, DSLVFragmentBGHandle caller)
                : base(caller.Activity, caller.getItemLayout(), Resource.Id.text, artists)
            {
                this.context = caller.Activity;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View v = base.GetView(position, convertView, parent);
                v.Background.SetLevel(3000);
                return v;
            }
        }

        private class MyDSController : DragSortController
        {
            DSLVFragmentBGHandle mCaller;
            DragSortListView mDslv;
            public MyDSController(DragSortListView dslv, DSLVFragmentBGHandle caller)
                : base(dslv)
            {
                setDragHandleId(Resource.Id.text);
                mDslv = dslv;
                mCaller = caller;
            }

            public override View OnCreateFloatView(int position)
            {
                View v = mCaller.ListAdapter.GetView(position, null, mDslv);
                v.Background.SetLevel(10000);
                return v;
            }

            public override void OnDestroyFloatView(View floatView)
            {
                //do nothing; block super from crashing
            }

            public override int StartDragPosition(MotionEvent ev)
            {
                int res = base.dragHandleHitPosition(ev);
                int width = mDslv.Width;

                if ((int)ev.GetX() < width / 3)
                {
                    return res;
                }
                else
                {
                    return DragSortController.MISS;
                }
            }
        }
    }
}