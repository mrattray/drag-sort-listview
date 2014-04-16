using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using DragSortListview;
using DragSortListview.Interfaces;

namespace SampleDSLV
{
    [Activity(Label = "Sections")]
    public class Sections : ListActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.sections_main);

            DragSortListView dslv = (DragSortListView)ListView;

            // get jazz artist names and make adapter
            String[] array = Resources.GetStringArray(Resource.Array.jazz_artist_names);
            SectionAdapter adapter = new SectionAdapter(this, new List<String>(array));
            dslv.SetDropListener(adapter);

            // make and set controller on dslv
            SectionController c = new SectionController(dslv, adapter);
            dslv.SetFloatViewManager(c);
            dslv.SetOnTouchListener(c);

            // pass it to the ListActivity
            ListAdapter = adapter;
        }

        private class SectionController : DragSortController
        {

            private int mPos;
            private int mDivPos;

            private SectionAdapter mAdapter;

            DragSortListView mDslv;

            public SectionController(DragSortListView dslv, SectionAdapter adapter)
                : base(dslv, Resource.Id.text, DragSortController.ON_DOWN, 0)
            {
                setRemoveEnabled(false);
                mDslv = dslv;
                mAdapter = adapter;
                mDivPos = adapter.getDivPosition();
            }

            public override int StartDragPosition(MotionEvent ev)
            {
                int res = base.dragHandleHitPosition(ev);
                if (res == mDivPos)
                {
                    return DragSortController.MISS;
                }

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

            public override View OnCreateFloatView(int position)
            {
                mPos = position;

                View v = mAdapter.GetView(position, null, mDslv);
                if (position < mDivPos)
                {
                    v.SetBackgroundDrawable(mDslv.Resources.GetDrawable(Resource.Drawable.bg_handle_section1));
                }
                else
                {
                    v.SetBackgroundDrawable(mDslv.Resources.GetDrawable(Resource.Drawable.bg_handle_section2));
                }
                v.Background.SetLevel(10000);
                return v;
            }

            private int origHeight = -1;

            public override void OnDragFloatView(View floatView, Point floatPoint, Point touchPoint)
            {
                int first = mDslv.FirstVisiblePosition;
                int lvDivHeight = mDslv.DividerHeight;

                if (origHeight == -1)
                {
                    origHeight = floatView.Height;
                }

                View div = mDslv.GetChildAt(mDivPos - first);

                if (touchPoint.X > mDslv.Width / 2)
                {
                    float scale = touchPoint.X - mDslv.Width / 2;
                    scale /= (float)(mDslv.Width / 5);
                    ViewGroup.LayoutParams lp = floatView.LayoutParameters;
                    lp.Height = Math.Max(origHeight, (int)(scale * origHeight));
                    Log.Debug("mobeta", "setting height " + lp.Height);
                    floatView.LayoutParameters = lp;
                }

                if (div != null)
                {
                    if (mPos > mDivPos)
                    {
                        // don't allow floating View to go above
                        // section divider
                        int limit = div.Bottom + lvDivHeight;
                        if (floatPoint.Y < limit)
                        {
                            floatPoint.Y = limit;
                        }
                    }
                    else
                    {
                        // don't allow floating View to go below
                        // section divider
                        int limit = div.Top - lvDivHeight - floatView.Height;
                        if (floatPoint.Y > limit)
                        {
                            floatPoint.Y = limit;
                        }
                    }
                }
            }

            public override void OnDestroyFloatView(View floatView)
            {
                //do nothing; block super from crashing
            }
        }

        private class SectionAdapter : BaseAdapter, IDropListener
        {

            private static int SECTION_DIV = 0;
            private static int SECTION_ONE = 1;
            private static int SECTION_TWO = 2;

            private List<String> mData;

            private int mDivPos;

            private LayoutInflater mInflater;
            private Context mContext;

            public SectionAdapter(Context context, List<String> names)
                : base()
            {
                mInflater = (LayoutInflater)context.GetSystemService(LayoutInflaterService);
                mData = names;
                mDivPos = names.Count() / 2;
                mContext = context;
            }

            public void Drop(int from, int to)
            {
                if (from != to)
                {
                    String data = mData[dataPosition(from)];
                    mData.RemoveAt(dataPosition(from));
                    mData.Insert(dataPosition(to), data);
                    NotifyDataSetChanged();
                }
            }

            public override int Count
            {
                get
                {
                    return mData.Count + 1;
                }
            }

            public override bool AreAllItemsEnabled()
            {
                return false;
            }

            public override bool IsEnabled(int position)
            {
                return position != mDivPos;
            }

            public int getDivPosition()
            {
                return mDivPos;
            }

            public override int ViewTypeCount
            {
                get
                {
                    return 3;
                }
            }
           
            public override Java.Lang.Object GetItem(int position)
            {
                if (position == mDivPos)
                {
                    return "Something";
                }
                else
                {
                    return mData[dataPosition(position)];
                }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override int GetItemViewType(int position)
            {
                if (position == mDivPos)
                {
                    return SECTION_DIV;
                }
                else if (position < mDivPos)
                {
                    return SECTION_ONE;
                }
                else
                {
                    return SECTION_TWO;
                }
            }

            private int dataPosition(int position)
            {
                return position > mDivPos ? position - 1 : position;
            }

            public Drawable getBGDrawable(int type)
            {
                Drawable d;
                if (type == SECTION_ONE)
                {
                    d = mContext.Resources.GetDrawable(Resource.Drawable.bg_handle_section1_selector);
                }
                else
                {
                    d = mContext.Resources.GetDrawable(Resource.Drawable.bg_handle_section2_selector);
                }
                d.SetLevel(3000);
                return d;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                int type = GetItemViewType(position);

                View v = null;
                if (convertView != null)
                {
                    Log.Debug("mobeta", "using convertView");
                    v = convertView;
                }
                else if (type != SECTION_DIV)
                {
                    Log.Debug("mobeta", "inflating normal item");
                    v = mInflater.Inflate(Resource.Layout.list_item_bg_handle, parent, false);
                    v.SetBackgroundDrawable(getBGDrawable(type));
                }
                else
                {
                    Log.Debug("mobeta", "inflating section divider");
                    v = mInflater.Inflate(Resource.Layout.section_div, parent, false);
                }

                if (type != SECTION_DIV)
                {
                    // bind data
                    ((TextView)v).Text = mData[dataPosition(position)];
                }

                return v;
            }
        }
    }
}