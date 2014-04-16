using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DragSortListview;
using DragSortListview.Helpers;
using DragSortListview.Interfaces;

namespace SampleDSLV
{
    public class DSLVFragment : ListFragment
    {

        public ArrayAdapter<String> adapter;

        private String[] array;
        private JavaList<String> list;

        protected int getLayout()
        {
            // this DSLV xml declaration does not call for the use
            // of the default DragSortController; therefore,
            // DSLVFragment has a buildController() method.
            return Resource.Layout.dslv_fragment_main;
        }

        /**
         * Return list item layout resource passed to the ArrayAdapter.
         */
        protected int getItemLayout()
        {
            /*if (removeMode == DragSortController.FLING_LEFT_REMOVE || removeMode == DragSortController.SLIDE_LEFT_REMOVE) {
                return R.layout.list_item_handle_right;
            } else */
            if (removeMode == DragSortController.CLICK_REMOVE)
            {
                return Resource.Layout.list_item_click_remove;
            }
            else
            {
                return Resource.Layout.list_item_handle_left;
            }
        }

        private DragSortListView mDslv;
        private DragSortController mController;

        public int dragStartMode = DragSortController.ON_DOWN;
        public bool removeEnabled = false;
        public int removeMode = DragSortController.FLING_REMOVE;
        public bool sortEnabled = true;
        public bool dragEnabled = true;

        public static DSLVFragment newInstance(int headers, int footers)
        {
            DSLVFragment f = new DSLVFragment();

            Bundle args = new Bundle();
            args.PutInt("headers", headers);
            args.PutInt("footers", footers);
            f.Arguments = args;

            return f;
        }

        public DragSortController getController()
        {
            return mController;
        }

        /**
         * Called from DSLVFragment.onActivityCreated(). Override to
         * set a different adapter.
         */
        public void setListAdapter()
        {
            array = Resources.GetStringArray(Resource.Array.jazz_artist_names);
            list = new JavaList<string>(array);
            adapter = new ArrayAdapter<string>(Activity, getItemLayout(), Resource.Id.text, list);
            ListAdapter = adapter;
        }

        /**
         * Called in onCreateView. Override this to provide a custom
         * DragSortController.
         */
        public virtual DragSortController BuildController(DragSortListView dslv)
        {
            // defaults are
            //   dragStartMode = onDown
            //   removeMode = flingRight
            DragSortController controller = new DragSortController(dslv);
            controller.setDragHandleId(Resource.Id.drag_handle);
            controller.setClickRemoveId(Resource.Id.click_remove);
            controller.setRemoveEnabled(removeEnabled);
            controller.setSortEnabled(sortEnabled);
            controller.setDragInitMode(dragStartMode);
            controller.setRemoveMode(removeMode);
            return controller;
        }


        /** Called when the activity is first created. */
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container,
                Bundle savedInstanceState)
        {
            mDslv = (DragSortListView)inflater.Inflate(getLayout(), container, false);

            mController = BuildController(mDslv);
            mDslv.SetFloatViewManager(mController);
            mDslv.SetOnTouchListener(mController);
            mDslv.setDragEnabled(dragEnabled);

            return mDslv;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            mDslv = (DragSortListView)ListView;

            Bundle args = Arguments;
            int headers = 0;
            int footers = 0;
            if (args != null)
            {
                headers = args.GetInt("headers", 0);
                footers = args.GetInt("footers", 0);
            }

            for (int i = 0; i < headers; i++)
            {
                addHeader(Activity, mDslv);
            }
            for (int i = 0; i < footers; i++)
            {
                addFooter(Activity, mDslv);
            }

            setListAdapter();
            mDslv.SetDropListener(new DropListener((int from, int to) =>
            {
                if (from != to)
                {
                    String item = adapter.GetItem(from);
                    adapter.Remove(item);
                    adapter.Insert(item, to);
                    adapter.NotifyDataSetChanged();
                }
            }));
            mDslv.SetRemoveListener(new RemoveListener((int which) =>
            {
                adapter.Remove(adapter.GetItem(which));
            }));
        }

        public static void addHeader(Activity activity, DragSortListView dslv)
        {
            LayoutInflater inflater = activity.LayoutInflater;
            int count = dslv.HeaderViewsCount;

            TextView header = (TextView)inflater.Inflate(Resource.Layout.header_footer, null);
            header.Text = "Header #" + (count + 1);

            dslv.AddHeaderView(header, null, false);
        }

        public static void addFooter(Activity activity, DragSortListView dslv)
        {
            LayoutInflater inflater = activity.LayoutInflater;
            int count = dslv.FooterViewsCount;

            TextView footer = (TextView)inflater.Inflate(Resource.Layout.header_footer, null);
            footer.Text = "Footer #" + (count + 1);

            dslv.AddFooterView(footer, null, false);
        }

    }
}