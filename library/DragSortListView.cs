using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DragSortListview.Interfaces;
using Java.IO;

namespace DragSortListview
{

        /*
 * DragSortListView.
 *
 * A subclass of the Android ListView component that enables drag
 * and drop re-ordering of list items.
 *
 * Copyright 2012 Carl Bauer
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * ListView subclass that mediates drag and drop resorting of items.
 * 
 * 
 * @author heycosmo
 *
 */
    public class DragSortListView : ListView
    {
        /**
         * The View that floats above the ListView and represents
         * the dragged item.
         */
        private View mFloatView;

        /**
         * The float View location. First based on touch location
         * and given deltaX and deltaY. Then restricted by callback
         * to FloatViewManager.onDragFloatView(). Finally restricted
         * by bounds of DSLV.
         */
        private Point mFloatLoc = new Point();

        private Point mTouchLoc = new Point();

        /**
         * The middle (in the y-direction) of the floating View.
         */
        private int mFloatViewMid;

        /**
         * Flag to make sure float View isn't measured twice
         */
        private bool mFloatViewOnMeasured = false;

        /**
         * Watch the Adapter for data changes. Cancel a drag if
         * coincident with a change.
         */
        private DataSetObserver mObserver;

        /**
         * Transparency for the floating View (XML attribute).
         */
        private float mFloatAlpha = 1.0f;
        private float mCurrFloatAlpha = 1.0f;

        /**
         * While drag-sorting, the current position of the floating
         * View. If dropped, the dragged item will land in this position.
         */
        private int mFloatPos;

        /**
         * The first expanded ListView position that helps represent
         * the drop slot tracking the floating View.
         */
        private int mFirstExpPos;

        /**
         * The second expanded ListView position that helps represent
         * the drop slot tracking the floating View. This can equal
         * mFirstExpPos if there is no slide shuffle occurring; otherwise
         * it is equal to mFirstExpPos + 1.
         */
        private int mSecondExpPos;

        /**
         * Flag set if slide shuffling is enabled.
         */
        private bool mAnimate = false;

        /**
         * The user dragged from this position.
         */
        private int mSrcPos;

        /**
         * Offset (in x) within the dragged item at which the user
         * picked it up (or first touched down with the digitalis).
         */
        private int mDragDeltaX;

        /**
         * Offset (in y) within the dragged item at which the user
         * picked it up (or first touched down with the digitalis).
         */
        private int mDragDeltaY;


        /**
         * The difference (in x) between screen coordinates and coordinates
         * in this view.
         */
        private int mOffsetX;

        /**
         * The difference (in y) between screen coordinates and coordinates
         * in this view.
         */
        private int mOffsetY;

        /**
         * A listener that receives callbacks whenever the floating View
         * hovers over a new position.
         */
        private IDragListener mDragListener;

        /**
         * A listener that receives a callback when the floating View
         * is dropped.
         */
        private IDropListener mDropListener;

        /**
         * A listener that receives a callback when the floating View
         * (or more precisely the originally dragged item) is removed
         * by one of the provided gestures.
         */
        private IRemoveListener mRemoveListener;

        /**
         * Enable/Disable item dragging
         * 
         * @attr name dslv:drag_enabled
         */
        private bool mDragEnabled = true;

        /**
         * Drag state enum.
         */
        private const int IDLE = 0;
        private const int REMOVING = 1;
        private const int DROPPING = 2;
        private const int STOPPED = 3;
        private const int DRAGGING = 4;

        private int mDragState = IDLE;

        /**
         * Height in pixels to which the originally dragged item
         * is collapsed during a drag-sort. Currently, this value
         * must be greater than zero.
         */
        private int mItemHeightCollapsed = 1;

        /**
         * Height of the floating View. Stored for the purpose of
         * providing the tracking drop slot.
         */
        private int mFloatViewHeight;

        /**
         * Convenience member. See above.
         */
        private int mFloatViewHeightHalf;

        /**
         * Save the given width spec for use in measuring children
         */
        private int mWidthMeasureSpec = 0;

        /**
         * Sample Views ultimately used for calculating the height
         * of ListView items that are off-screen.
         */
        private View[] mSampleViewTypes = new View[1];

        /**
         * Drag-scroll encapsulator!
         */
        private DragScroller mDragScroller;

        /**
         * Determines the start of the upward drag-scroll region
         * at the top of the ListView. Specified by a fraction
         * of the ListView height, thus screen resolution agnostic.
         */
        private float mDragUpScrollStartFrac = 1.0f / 3.0f;

        /**
         * Determines the start of the downward drag-scroll region
         * at the bottom of the ListView. Specified by a fraction
         * of the ListView height, thus screen resolution agnostic.
         */
        private float mDragDownScrollStartFrac = 1.0f / 3.0f;

        /**
         * The following are calculated from the above fracs.
         */
        private int mUpScrollStartY;
        private int mDownScrollStartY;
        private float mDownScrollStartYF;
        private float mUpScrollStartYF;

        /**
         * Calculated from above above and current ListView height.
         */
        private float mDragUpScrollHeight;

        /**
         * Calculated from above above and current ListView height.
         */
        private float mDragDownScrollHeight;

        /**
         * Defines the scroll speed during a drag-scroll. User can
         * provide their own; this default is a simple linear profile
         * where scroll speed increases linearly as the floating View
         * nears the top/bottom of the ListView.
         */
        public class DragScrollProfile : IDragScrollProfile
        {
            /**
             * Maximum drag-scroll speed in pixels per ms. Only used with
             * default linear drag-scroll profile.
             */
            public float mMaxScrollSpeed = 0.5f;

            public virtual float GetSpeed(float w, long t)
            {
                return mMaxScrollSpeed * w;
            }
        }

        public DragScrollProfile mScrollProfile = new DragScrollProfile();
        /**
         * Current touch x.
         */
        private int mX;

        /**
         * Current touch y.
         */
        private int mY;

        /**
         * Last touch x.
         */
        private int mLastX;

        /**
         * Last touch y.
         */
        private int mLastY;

        /**
         * The touch y-coord at which drag started
         */
        private int mDragStartY;

        /**
         * Drag flag bit. Floating View can move in the positive
         * x direction.
         */
        public const int DRAG_POS_X = 0x1;

        /**
         * Drag flag bit. Floating View can move in the negative
         * x direction.
         */
        public const int DRAG_NEG_X = 0x2;

        /**
         * Drag flag bit. Floating View can move in the positive
         * y direction. This is subtle. What this actually means is
         * that, if enabled, the floating View can be dragged below its starting
         * position. Remove in favor of upper-bounding item position?
         */
        public const int DRAG_POS_Y = 0x4;

        /**
         * Drag flag bit. Floating View can move in the negative
         * y direction. This is subtle. What this actually means is
         * that the floating View can be dragged above its starting
         * position. Remove in favor of lower-bounding item position?
         */
        public const int DRAG_NEG_Y = 0x8;

        /**
         * Flags that determine limits on the motion of the
         * floating View. See flags above.
         */
        private int mDragFlags = 0;

        /**
         * Last call to an on*TouchEvent was a call to
         * onInterceptTouchEvent.
         */
        private bool mLastCallWasIntercept = false;

        /**
         * A touch event is in progress.
         */
        private bool mInTouchEvent = false;

        /**
         * Let the user customize the floating View.
         */
        private IFloatViewManager mFloatViewManager = null;

        /**
         * Given to ListView to cancel its action when a drag-sort
         * begins.
         */
        private MotionEvent mCancelEvent;

        /**
         * Enum telling where to cancel the ListView action when a
         * drag-sort begins
         */
        private const int NO_CANCEL = 0;
        private const int ON_TOUCH_EVENT = 1;
        private const int ON_INTERCEPT_TOUCH_EVENT = 2;

        /**
         * Where to cancel the ListView action when a
         * drag-sort begins
         */
        private int mCancelMethod = NO_CANCEL;

        /**
         * Determines when a slide shuffle animation starts. That is,
         * defines how close to the edge of the drop slot the floating
         * View must be to initiate the slide.
         */
        private float mSlideRegionFrac = 0.25f;

        /**
         * Number between 0 and 1 indicating the relative location of
         * a sliding item (only used if drag-sort animations
         * are turned on). Nearly 1 means the item is 
         * at the top of the slide region (nearly full blank item
         * is directly below).
         */
        private float mSlideFrac = 0.0f;

        /**
         * Wraps the user-provided ListAdapter. This is used to wrap each
         * item View given by the user inside another View (currenly
         * a RelativeLayout) which
         * expands and collapses to simulate the item shuffling.
         */
        private AdapterWrapper mAdapterWrapper;

        /**
         * Turn on custom debugger.
         */
        private bool mTrackDragSort = false;

        /**
         * Debugging class.
         */
        private DragSortTracker mDragSortTracker;

        /**
         * Needed for adjusting item heights from within layoutChildren
         */
        private bool mBlockLayoutRequests = false;

        /**
         * Set to true when a down event happens during drag sort;
         * for example, when drag finish animations are
         * playing.
         */
        private bool mIgnoreTouchEvent = false;

        /**
         * Caches DragSortItemView child heights. Sometimes DSLV has to
         * know the height of an offscreen item. Since ListView virtualizes
         * these, DSLV must get the item from the ListAdapter to obtain
         * its height. That process can be expensive, but often the same
         * offscreen item will be requested many times in a row. Once an
         * offscreen item height is calculated, we cache it in this guy.
         * Actually, we cache the height of the child of the
         * DragSortItemView since the item height changes often during a
         * drag-sort.
         */
        private static int sCacheSize = 3;
        private HeightCache mChildHeightCache = new HeightCache(sCacheSize);

        private RemoveAnimator mRemoveAnimator;

        private LiftAnimator mLiftAnimator;

        private DropAnimator mDropAnimator;

        private bool mUseRemoveVelocity;
        private float mRemoveVelocityX = 0;

        public DragSortListView(IntPtr jr, JniHandleOwnership handle)
            : base(jr, handle)
        {
        }

        public DragSortListView(Context context)
            : base(context)
        {
            BaseCreation(null);
        }

        public DragSortListView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            BaseCreation(attrs);            
        }

        public DragSortListView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            BaseCreation(attrs);
        }

        private void BaseCreation(IAttributeSet attrs)
        {
            int defaultDuration = 150;
            int removeAnimDuration = defaultDuration; // ms
            int dropAnimDuration = defaultDuration; // ms

            if (attrs != null)
            {
                TypedArray a = Context.ObtainStyledAttributes(
                        attrs,
                        Resource.Styleable.DragSortListView, 0, 0);

                mItemHeightCollapsed = Math.Max(1, a.GetDimensionPixelSize(
                        Resource.Styleable.DragSortListView_collapsed_height, 1));

                mTrackDragSort = a.GetBoolean(
                        Resource.Styleable.DragSortListView_track_drag_sort, false);

                if (mTrackDragSort)
                {
                    mDragSortTracker = new DragSortTracker(this);
                }

                // alpha between 0 and 255, 0=transparent, 255=opaque
                mFloatAlpha = a.GetFloat(Resource.Styleable.DragSortListView_float_alpha, mFloatAlpha);
                mCurrFloatAlpha = mFloatAlpha;

                mDragEnabled = a.GetBoolean(Resource.Styleable.DragSortListView_drag_enabled, mDragEnabled);

                mSlideRegionFrac = Math.Max(0.0f,
                        Math.Min(1.0f, 1.0f - a.GetFloat(
                                Resource.Styleable.DragSortListView_slide_shuffle_speed,
                                0.75f)));

                mAnimate = mSlideRegionFrac > 0.0f;

                float frac = a.GetFloat(
                        Resource.Styleable.DragSortListView_drag_scroll_start,
                        mDragUpScrollStartFrac);

                setDragScrollStart(frac);

                mScrollProfile.mMaxScrollSpeed = a.GetFloat(
                        Resource.Styleable.DragSortListView_max_drag_scroll_speed,
                        mScrollProfile.mMaxScrollSpeed);

                removeAnimDuration = a.GetInt(
                        Resource.Styleable.DragSortListView_remove_animation_duration,
                        removeAnimDuration);

                dropAnimDuration = a.GetInt(
                        Resource.Styleable.DragSortListView_drop_animation_duration,
                        dropAnimDuration);

                bool useDefault = a.GetBoolean(
                        Resource.Styleable.DragSortListView_use_default_controller,
                        true);

                if (useDefault)
                {
                    bool removeEnabled = a.GetBoolean(
                            Resource.Styleable.DragSortListView_remove_enabled,
                            false);
                    int removeMode = a.GetInt(
                            Resource.Styleable.DragSortListView_remove_mode,
                            DragSortController.FLING_REMOVE);
                    bool sortEnabled = a.GetBoolean(
                            Resource.Styleable.DragSortListView_sort_enabled,
                            true);
                    int dragInitMode = a.GetInt(
                            Resource.Styleable.DragSortListView_drag_start_mode,
                            DragSortController.ON_DOWN);
                    int dragHandleId = a.GetResourceId(
                            Resource.Styleable.DragSortListView_drag_handle_id,
                            0);
                    int flingHandleId = a.GetResourceId(
                            Resource.Styleable.DragSortListView_fling_handle_id,
                            0);
                    int clickRemoveId = a.GetResourceId(
                            Resource.Styleable.DragSortListView_click_remove_id,
                            0);
                    Color bgColor = a.GetColor(
                            Resource.Styleable.DragSortListView_float_background_color,
                            Color.Black);

                    DragSortController controller = new DragSortController(
                            this, dragHandleId, dragInitMode, removeMode,
                            clickRemoveId, flingHandleId);
                    controller.setRemoveEnabled(removeEnabled);
                    controller.setSortEnabled(sortEnabled);
                    controller.setBackgroundColor(bgColor);

                    mFloatViewManager = controller;
                    SetOnTouchListener(controller);
                }

                a.Recycle();
            }

            mDragScroller = new DragScroller(this);

            float smoothness = 0.5f;
            if (removeAnimDuration > 0)
            {
                mRemoveAnimator = new RemoveAnimator(this, smoothness, removeAnimDuration);
            }
            // mLiftAnimator = new LiftAnimator(smoothness, 100);
            if (dropAnimDuration > 0)
            {
                mDropAnimator = new DropAnimator(this, smoothness, dropAnimDuration);
            }

            mCancelEvent = MotionEvent.Obtain(0, 0, MotionEventActions.Cancel, 0f, 0f, 0f, 0f, 0, 0f,
                    0f, 0, 0);

            // construct the dataset observer
            mObserver = new SimpleDataSetObserver(cancelDrag, mDragState);
        }

        private class SimpleDataSetObserver : DataSetObserver
        {
            Action cancelDrag;
            int mDragState;
            public SimpleDataSetObserver(Action cancelDrag, int mDragState)
            {
                this.cancelDrag = cancelDrag;
                this.mDragState = mDragState;
            }

            private void Cancel()
            {
                if (mDragState == DRAGGING)
                {
                    cancelDrag();
                }
            }

            public override void OnChanged()
            {
                Cancel();
            }

            public override void OnInvalidated()
            {
                Cancel();
            }
        }

        /**
         * Usually called from a FloatViewManager. The float alpha
         * will be reset to the xml-defined value every time a drag
         * is stopped.
         */
        public void setFloatAlpha(float alpha)
        {
            mCurrFloatAlpha = alpha;
        }

        public float getFloatAlpha()
        {
            return mCurrFloatAlpha;
        }

        /**
         * Set maximum drag scroll speed in positions/second. Only applies
         * if using default ScrollSpeedProfile.
         * 
         * @param max Maximum scroll speed.
         */
        public void setMaxScrollSpeed(float max)
        {
            mScrollProfile.mMaxScrollSpeed = max;
        }

        /**
         * For each DragSortListView Listener interface implemented by
         * <code>adapter</code>, this method calls the appropriate
         * set*Listener method with <code>adapter</code> as the argument.
         * 
         * @param adapter The ListAdapter providing data to back
         * DragSortListView.
         *
         * @see android.widget.ListView#setAdapter(android.widget.ListAdapter)
         */
        public override IListAdapter Adapter
        {
            get
            {
                return base.Adapter;
            }
            set
            {
                if (value != null)
                {
                    mAdapterWrapper = new AdapterWrapper(this, value);
                    value.RegisterDataSetObserver(mObserver);
                    
                    if (value is IDropListener)
                    {
                        SetDropListener((IDropListener)value);
                    }
                    if (value is IDragListener)
                    {
                        SetDragListener((IDragListener)value);
                    }
                    if (value is IRemoveListener)
                    {
                        SetRemoveListener((IRemoveListener)value);
                    }
                }
                else
                {
                    mAdapterWrapper = null;
                }

                base.Adapter = mAdapterWrapper;
            }
        }

        /**
         * As opposed to {@link ListView#getAdapter()}, which returns
         * a heavily wrapped ListAdapter (DragSortListView wraps the
         * input ListAdapter {\emph and} ListView wraps the wrapped one).
         *
         * @return The ListAdapter set as the argument of {@link setAdapter()}
         */
        public IListAdapter getInputAdapter()
        {
            if (mAdapterWrapper == null)
            {
                return null;
            }
            else
            {
                return mAdapterWrapper.getAdapter();
            }
        }

        private class MyDataSetObserver : DataSetObserver
        {
            private AdapterWrapper myWrapper;
            public MyDataSetObserver(AdapterWrapper wrapper)
            {
                myWrapper = wrapper;
            }

            public override void OnChanged()
            {
                myWrapper.NotifyDataSetChanged();
            }

            public override void OnInvalidated()
            {
                myWrapper.NotifyDataSetInvalidated();
            }
        }

        private class AdapterWrapper : BaseAdapter
        {
            private DragSortListView dslv;
            private IListAdapter mAdapter;

            public AdapterWrapper(DragSortListView dslv, IListAdapter adapter)
                : base()
            {
                this.dslv = dslv;
                mAdapter = adapter;

                mAdapter.RegisterDataSetObserver(new MyDataSetObserver(this));
            }

            public IListAdapter getAdapter()
            {
                return mAdapter;
            }

            public override long GetItemId(int position)
            {
                return mAdapter.GetItemId(position);
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return mAdapter.GetItem(position);
            }

            public override int Count
            {
                get { return mAdapter.Count; }
            }

            public override bool AreAllItemsEnabled()
            {
                return mAdapter.AreAllItemsEnabled();
            }

            public override bool IsEnabled(int position)
            {
                return mAdapter.IsEnabled(position);
            }

            public override int GetItemViewType(int position)
            {
                return mAdapter.GetItemViewType(position);
            }

            public override int ViewTypeCount
            {
                get
                {
                    return mAdapter.ViewTypeCount;
                }
            }

            public override bool HasStableIds
            {
                get
                {
                    return mAdapter.HasStableIds;
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    return mAdapter.IsEmpty;
                }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {

                DragSortItemView v;
                View child;
                // Log.d("mobeta",
                // "getView: position="+position+" convertView="+convertView);
                if (convertView != null)
                {
                    v = (DragSortItemView)convertView;
                    View oldChild = v.GetChildAt(0);

                    child = mAdapter.GetView(position, oldChild, parent);
                    if (child != oldChild)
                    {
                        // shouldn't get here if user is reusing convertViews
                        // properly
                        if (oldChild != null)
                        {
                            v.RemoveViewAt(0);
                        }
                        v.AddView(child);
                    }
                }
                else
                {
                    child = mAdapter.GetView(position, null, parent);
                    if (child is ICheckable)
                    {
                        v = new DragSortItemViewCheckable(parent.Context);
                    }
                    else
                    {
                        v = new DragSortItemView(parent.Context);
                    }
                    v.LayoutParameters = new AbsListView.LayoutParams(
                            ViewGroup.LayoutParams.FillParent,
                            ViewGroup.LayoutParams.WrapContent);
                    v.AddView(child);
                }

                // Set the correct item height given drag state; passed
                // View needs to be measured if measurement is required.
                dslv.adjustItem(position + dslv.HeaderViewsCount, v, true);

                return v;
            }
        }

        private void drawDivider(int expPosition, Canvas canvas)
        {

            Drawable divider = Divider;
            int dividerHeight = DividerHeight;
            // Log.d("mobeta", "div="+divider+" divH="+dividerHeight);

            if (divider != null && dividerHeight != 0)
            {
                ViewGroup expItem = (ViewGroup)GetChildAt(expPosition
                        - FirstVisiblePosition);
                if (expItem != null)
                {
                    int l = PaddingLeft;
                    int r = Width - PaddingRight;
                    int t;
                    int b;

                    int childHeight = expItem.GetChildAt(0).Height;

                    if (expPosition > mSrcPos)
                    {
                        t = expItem.Top + childHeight;
                        b = t + dividerHeight;
                    }
                    else
                    {
                        b = expItem.Bottom - childHeight;
                        t = b - dividerHeight;
                    }
                    // Log.d("mobeta", "l="+l+" t="+t+" r="+r+" b="+b);

                    // Have to clip to support ColorDrawable on <= Gingerbread
                    canvas.Save();
                    canvas.ClipRect(l, t, r, b);
                    divider.SetBounds(l, t, r, b);
                    divider.Draw(canvas);
                    canvas.Restore();
                }
            }
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            base.DispatchDraw(canvas);

            if (mDragState != IDLE)
            {
                // draw the divider over the expanded item
                if (mFirstExpPos != mSrcPos)
                {
                    drawDivider(mFirstExpPos, canvas);
                }
                if (mSecondExpPos != mFirstExpPos && mSecondExpPos != mSrcPos)
                {
                    drawDivider(mSecondExpPos, canvas);
                }
            }

            if (mFloatView != null)
            {
                // draw the float view over everything
                int w = mFloatView.Width;
                int h = mFloatView.Height;

                int x = mFloatLoc.X;

                int width = Width;
                if (x < 0)
                    x = -x;
                float alphaMod;
                if (x < width)
                {
                    alphaMod = ((float)(width - x)) / ((float)width);
                    alphaMod *= alphaMod;
                }
                else
                {
                    alphaMod = 0;
                }

                int alpha = (int)(255f * mCurrFloatAlpha * alphaMod);

                canvas.Save();
                // Log.d("mobeta", "clip rect bounds: " + canvas.getClipBounds());
                canvas.Translate(mFloatLoc.X, mFloatLoc.Y);
                canvas.ClipRect(0, 0, w, h);

                // Log.d("mobeta", "clip rect bounds: " + canvas.getClipBounds());
                canvas.SaveLayerAlpha(0, 0, w, h, alpha, SaveFlags.All);
                mFloatView.Draw(canvas);
                canvas.Restore();
                canvas.Restore();
            }
        }

        private int getItemHeight(int position)
        {
            View v = GetChildAt(position - FirstVisiblePosition);

            if (v != null)
            {
                // item is onscreen, just get the height of the View
                return v.Height;
            }
            else
            {
                // item is offscreen. get child height and calculate
                // item height based on current shuffle state
                return calcItemHeight(position, getChildHeight(position));
            }
        }

        private void printPosData()
        {
            Log.Debug("mobeta", "mSrcPos=" + mSrcPos + " mFirstExpPos=" + mFirstExpPos + " mSecondExpPos="
                    + mSecondExpPos);
        }

        private class HeightCache
        {

            private SparseIntArray mMap;
            private List<int> mOrder;
            private int mMaxSize;

            public HeightCache(int size)
            {
                mMap = new SparseIntArray(size);
                mOrder = new List<int>(size);
                mMaxSize = size;
            }

            /**
             * Add item height at position if doesn't already exist.
             */
            public void add(int position, int height)
            {
                int currHeight = mMap.Get(position, -1);
                if (currHeight != height)
                {
                    if (currHeight == -1)
                    {
                        if (mMap.Size() == mMaxSize)
                        {
                            // remove oldest entry
                            int item = mOrder[0];
                            mOrder.RemoveAt(0);
                            mMap.Delete(item);
                        }
                    }
                    else
                    {
                        // move position to newest slot
                        mOrder.Remove((int)position);
                    }
                    mMap.Put(position, height);
                    mOrder.Add(position);
                }
            }

            public int get(int position)
            {
                return mMap.Get(position, -1);
            }

            public void clear()
            {
                mMap.Clear();
                mOrder.Clear();
            }

        }

        /**
         * Get the shuffle edge for item at position when top of
         * item is at y-coord top. Assumes that current item heights
         * are consistent with current float view location and
         * thus expanded positions and slide fraction. i.e. Should not be
         * called between update of expanded positions/slide fraction
         * and layoutChildren.
         *
         * @param position 
         * @param top
         * @param height Height of item at position. If -1, this function
         * calculates this height.
         *
         * @return Shuffle line between position-1 and position (for
         * the given view of the list; that is, for when top of item at
         * position has y-coord of given `top`). If
         * floating View (treated as horizontal line) is dropped
         * immediately above this line, it lands in position-1. If
         * dropped immediately below this line, it lands in position.
         */
        private int getShuffleEdge(int position, int top)
        {

            int numHeaders = HeaderViewsCount;
            int numFooters = FooterViewsCount;

            // shuffle edges are defined between items that can be
            // dragged; there are N-1 of them if there are N draggable
            // items.

            if (position <= numHeaders || (position >= Count - numFooters))
            {
                return top;
            }

            int divHeight = DividerHeight;

            int edge;

            int maxBlankHeight = mFloatViewHeight - mItemHeightCollapsed;
            int childHeight = getChildHeight(position);
            int itemHeight = getItemHeight(position);

            // first calculate top of item given that floating View is
            // centered over src position
            int otop = top;
            if (mSecondExpPos <= mSrcPos)
            {
                // items are expanded on and/or above the source position

                if (position == mSecondExpPos && mFirstExpPos != mSecondExpPos)
                {
                    if (position == mSrcPos)
                    {
                        otop = top + itemHeight - mFloatViewHeight;
                    }
                    else
                    {
                        int blankHeight = itemHeight - childHeight;
                        otop = top + blankHeight - maxBlankHeight;
                    }
                }
                else if (position > mSecondExpPos && position <= mSrcPos)
                {
                    otop = top - maxBlankHeight;
                }

            }
            else
            {
                // items are expanded on and/or below the source position

                if (position > mSrcPos && position <= mFirstExpPos)
                {
                    otop = top + maxBlankHeight;
                }
                else if (position == mSecondExpPos && mFirstExpPos != mSecondExpPos)
                {
                    int blankHeight = itemHeight - childHeight;
                    otop = top + blankHeight;
                }
            }

            // otop is set
            if (position <= mSrcPos)
            {
                edge = otop + (mFloatViewHeight - divHeight - getChildHeight(position - 1)) / 2;
            }
            else
            {
                edge = otop + (childHeight - divHeight - mFloatViewHeight) / 2;
            }

            return edge;
        }

        private bool updatePositions()
        {

            int first = FirstVisiblePosition;
            int startPos = mFirstExpPos;
            View startView = GetChildAt(startPos - first);

            if (startView == null)
            {
                startPos = first + ChildCount / 2;
                startView = GetChildAt(startPos - first);
            }
            int startTop = startView.Top;

            int itemHeight = startView.Height;

            int edge = getShuffleEdge(startPos, startTop);
            int lastEdge = edge;

            int divHeight = DividerHeight;

            // Log.d("mobeta", "float mid="+mFloatViewMid);

            int itemPos = startPos;
            int itemTop = startTop;
            if (mFloatViewMid < edge)
            {
                // scanning up for float position
                // Log.d("mobeta", "    edge="+edge);
                while (itemPos >= 0)
                {
                    itemPos--;
                    itemHeight = getItemHeight(itemPos);

                    if (itemPos == 0)
                    {
                        edge = itemTop - divHeight - itemHeight;
                        break;
                    }

                    itemTop -= itemHeight + divHeight;
                    edge = getShuffleEdge(itemPos, itemTop);
                    // Log.d("mobeta", "    edge="+edge);

                    if (mFloatViewMid >= edge)
                    {
                        break;
                    }

                    lastEdge = edge;
                }
            }
            else
            {
                // scanning down for float position
                // Log.d("mobeta", "    edge="+edge);
                int count = Count;
                while (itemPos < count)
                {
                    if (itemPos == count - 1)
                    {
                        edge = itemTop + divHeight + itemHeight;
                        break;
                    }

                    itemTop += divHeight + itemHeight;
                    itemHeight = getItemHeight(itemPos + 1);
                    edge = getShuffleEdge(itemPos + 1, itemTop);
                    // Log.d("mobeta", "    edge="+edge);

                    // test for hit
                    if (mFloatViewMid < edge)
                    {
                        break;
                    }

                    lastEdge = edge;
                    itemPos++;
                }
            }

            int numHeaders = HeaderViewsCount;
            int numFooters = FooterViewsCount;

            bool updated = false;

            int oldFirstExpPos = mFirstExpPos;
            int oldSecondExpPos = mSecondExpPos;
            float oldSlideFrac = mSlideFrac;

            if (mAnimate)
            {
                int edgeToEdge = Math.Abs(edge - lastEdge);

                int edgeTop, edgeBottom;
                if (mFloatViewMid < edge)
                {
                    edgeBottom = edge;
                    edgeTop = lastEdge;
                }
                else
                {
                    edgeTop = edge;
                    edgeBottom = lastEdge;
                }
                // Log.d("mobeta", "edgeTop="+edgeTop+" edgeBot="+edgeBottom);

                int slideRgnHeight = (int)(0.5f * mSlideRegionFrac * edgeToEdge);
                float slideRgnHeightF = (float)slideRgnHeight;
                int slideEdgeTop = edgeTop + slideRgnHeight;
                int slideEdgeBottom = edgeBottom - slideRgnHeight;

                // Three regions
                if (mFloatViewMid < slideEdgeTop)
                {
                    mFirstExpPos = itemPos - 1;
                    mSecondExpPos = itemPos;
                    mSlideFrac = 0.5f * ((float)(slideEdgeTop - mFloatViewMid)) / slideRgnHeightF;
                    // Log.d("mobeta",
                    // "firstExp="+mFirstExpPos+" secExp="+mSecondExpPos+" slideFrac="+mSlideFrac);
                }
                else if (mFloatViewMid < slideEdgeBottom)
                {
                    mFirstExpPos = itemPos;
                    mSecondExpPos = itemPos;
                }
                else
                {
                    mFirstExpPos = itemPos;
                    mSecondExpPos = itemPos + 1;
                    mSlideFrac = 0.5f * (1.0f + ((float)(edgeBottom - mFloatViewMid))
                            / slideRgnHeightF);
                    // Log.d("mobeta",
                    // "firstExp="+mFirstExpPos+" secExp="+mSecondExpPos+" slideFrac="+mSlideFrac);
                }

            }
            else
            {
                mFirstExpPos = itemPos;
                mSecondExpPos = itemPos;
            }

            // correct for headers and footers
            if (mFirstExpPos < numHeaders)
            {
                itemPos = numHeaders;
                mFirstExpPos = itemPos;
                mSecondExpPos = itemPos;
            }
            else if (mSecondExpPos >= this.Count - numFooters)
            {
                itemPos = this.Count - numFooters - 1;
                mFirstExpPos = itemPos;
                mSecondExpPos = itemPos;
            }

            if (mFirstExpPos != oldFirstExpPos || mSecondExpPos != oldSecondExpPos
                    || mSlideFrac != oldSlideFrac)
            {
                updated = true;
            }

            if (itemPos != mFloatPos)
            {
                if (mDragListener != null)
                {
                    mDragListener.Drag(mFloatPos - numHeaders, itemPos - numHeaders);
                }

                mFloatPos = itemPos;
                updated = true;
            }

            return updated;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (mTrackDragSort)
            {
                mDragSortTracker.appendState();
            }
        }

        private class SmoothAnimator : Java.Lang.Object, Java.Lang.IRunnable
        {
            protected DragSortListView dslv;
            protected long mStartTime;

            private float mDurationF;

            private float mAlpha;
            private float mA, mB, mC, mD;

            private bool mCanceled;

            public SmoothAnimator(DragSortListView dslv, float smoothness, int duration)
            {
                this.dslv = dslv;
                mAlpha = smoothness;
                mDurationF = (float)duration;
                mA = mD = 1f / (2f * mAlpha * (1f - mAlpha));
                mB = mAlpha / (2f * (mAlpha - 1f));
                mC = 1f / (1f - mAlpha);
            }

            public float transform(float frac)
            {
                if (frac < mAlpha)
                {
                    return mA * frac * frac;
                }
                else if (frac < 1f - mAlpha)
                {
                    return mB + mC * frac;
                }
                else
                {
                    return 1f - mD * (frac - 1f) * (frac - 1f);
                }
            }

            public void start()
            {
                mStartTime = SystemClock.UptimeMillis();
                mCanceled = false;
                OnStart();
                dslv.Post(this);
            }

            public void cancel()
            {
                mCanceled = true;
            }

            public virtual void OnStart()
            {
                // stub
            }

            public virtual void OnUpdate(float frac, float smoothFrac)
            {
                // stub
            }

            public virtual void OnStop()
            {
                // stub
            }

            public void Run()
            {
                if (mCanceled)
                {
                    return;
                }

                float fraction = ((float)(SystemClock.UptimeMillis() - mStartTime)) / mDurationF;

                if (fraction >= 1f)
                {
                    OnUpdate(1f, 1f);
                    OnStop();
                }
                else
                {
                    OnUpdate(fraction, transform(fraction));
                    dslv.Post(this);
                }
            }
        }

        /**
         * Centers floating View under touch point.
         */
        private class LiftAnimator : SmoothAnimator
        {
            private float mInitDragDeltaY;
            private float mFinalDragDeltaY;

            public LiftAnimator(DragSortListView dslv, float smoothness, int duration)
                : base(dslv, smoothness, duration)
            {
            }

            public override void OnStart()
            {
                mInitDragDeltaY = dslv.mDragDeltaY;
                mFinalDragDeltaY = dslv.mFloatViewHeightHalf;
            }

            public override void OnUpdate(float frac, float smoothFrac)
            {
                if (dslv.mDragState != DRAGGING)
                {
                    cancel();
                }
                else
                {
                    dslv.mDragDeltaY = (int)(smoothFrac * mFinalDragDeltaY + (1f - smoothFrac)
                            * mInitDragDeltaY);
                    dslv.mFloatLoc.Y = dslv.mY - dslv.mDragDeltaY;
                    dslv.doDragFloatView(true);
                }
            }
        }

        /**
         * Centers floating View over drop slot before destroying.
         */
        private class DropAnimator : SmoothAnimator
        {

            private int mDropPos;
            private int srcPos;
            private float mInitDeltaY;
            private float mInitDeltaX;

            public DropAnimator(DragSortListView dslv, float smoothness, int duration)
                : base(dslv, smoothness, duration)
            {
            }

            public override void OnStart()
            {
                mDropPos = dslv.mFloatPos;
                srcPos = dslv.mSrcPos;
                dslv.mDragState = DROPPING;
                mInitDeltaY = dslv.mFloatLoc.Y - GetTargetY();
                mInitDeltaX = dslv.mFloatLoc.X - dslv.PaddingLeft;
            }

            private int GetTargetY()
            {
                int first = dslv.FirstVisiblePosition;
                int otherAdjust = (dslv.mItemHeightCollapsed + dslv.DividerHeight) / 2;
                View v = dslv.GetChildAt(mDropPos - first);
                int targetY = -1;
                if (v != null)
                {
                    if (mDropPos == srcPos)
                    {
                        targetY = v.Top;
                    }
                    else if (mDropPos < srcPos)
                    {
                        // expanded down
                        targetY = v.Top - otherAdjust;
                    }
                    else
                    {
                        // expanded up
                        targetY = v.Bottom + otherAdjust - dslv.mFloatViewHeight;
                    }
                }
                else
                {
                    // drop position is not on screen?? no animation
                    cancel();
                }

                return targetY;
            }

            public override void OnUpdate(float frac, float smoothFrac)
            {
                int targetY = GetTargetY();
                int targetX = dslv.Left;
                float deltaY = dslv.mFloatLoc.Y - targetY;
                float deltaX = dslv.mFloatLoc.X - targetX;
                float f = 1f - smoothFrac;
                if (f < Math.Abs(deltaY / mInitDeltaY) || f < Math.Abs(deltaX / mInitDeltaX))
                {
                    dslv.mFloatLoc.Y = targetY + (int)(mInitDeltaY * f);
                    dslv.mFloatLoc.X = dslv.PaddingLeft + (int)(mInitDeltaX * f);
                    dslv.doDragFloatView(true);
                }
            }

            public override void OnStop()
            {
                dslv.dropFloatView();
            }
        }

        /**
         * Collapses expanded items.
         */
        private class RemoveAnimator : SmoothAnimator
        {

            private float mFloatLocX;
            private float mFirstStartBlank;
            private float mSecondStartBlank;

            private int mFirstChildHeight = -1;
            private int mSecondChildHeight = -1;

            private int mFirstPos;
            private int mSecondPos;
            private int srcPos;

            public RemoveAnimator(DragSortListView dslv, float smoothness, int duration)
                : base(dslv, smoothness, duration)
            {
            }

            public override void OnStart()
            {
                mFirstChildHeight = -1;
                mSecondChildHeight = -1;
                mFirstPos = dslv.mFirstExpPos;
                mSecondPos = dslv.mSecondExpPos;
                srcPos = dslv.mSrcPos;
                dslv.mDragState = REMOVING;

                mFloatLocX = dslv.mFloatLoc.X;
                if (dslv.mUseRemoveVelocity)
                {
                    float minVelocity = 2f * dslv.Width;
                    if (dslv.mRemoveVelocityX == 0)
                    {
                        dslv.mRemoveVelocityX = (mFloatLocX < 0 ? -1 : 1) * minVelocity;
                    }
                    else
                    {
                        minVelocity *= 2;
                        if (dslv.mRemoveVelocityX < 0 && dslv.mRemoveVelocityX > -minVelocity)
                            dslv.mRemoveVelocityX = -minVelocity;
                        else if (dslv.mRemoveVelocityX > 0 && dslv.mRemoveVelocityX < minVelocity)
                            dslv.mRemoveVelocityX = minVelocity;
                    }
                }
                else
                {
                    dslv.destroyFloatView();
                }
            }

            public override void OnUpdate(float frac, float smoothFrac)
            {
                float f = 1f - smoothFrac;

                int firstVis = dslv.FirstVisiblePosition;
                View item = dslv.GetChildAt(mFirstPos - firstVis);
                ViewGroup.LayoutParams lp;
                int blank;

                if (dslv.mUseRemoveVelocity)
                {
                    float dt = (float)(SystemClock.UptimeMillis() - mStartTime) / 1000;
                    if (dt == 0)
                        return;
                    float dx = dslv.mRemoveVelocityX * dt;
                    int w = dslv.Width;
                    dslv.mRemoveVelocityX += (dslv.mRemoveVelocityX > 0 ? 1 : -1) * dt * w;
                    mFloatLocX += dx;
                    dslv.mFloatLoc.X = (int)mFloatLocX;
                    if (mFloatLocX < w && mFloatLocX > -w)
                    {
                        mStartTime = SystemClock.UptimeMillis();
                        dslv.doDragFloatView(true);
                        return;
                    }
                }

                if (item != null)
                {
                    if (mFirstChildHeight == -1)
                    {
                        mFirstChildHeight = dslv.getChildHeight(mFirstPos, item, false);
                        mFirstStartBlank = (float)(item.Height - mFirstChildHeight);
                    }
                    blank = Math.Max((int)(f * mFirstStartBlank), 1);
                    lp = item.LayoutParameters;
                    lp.Height = mFirstChildHeight + blank;
                    item.LayoutParameters = lp;
                }
                if (mSecondPos != mFirstPos)
                {
                    item = dslv.GetChildAt(mSecondPos - firstVis);
                    if (item != null)
                    {
                        if (mSecondChildHeight == -1)
                        {
                            mSecondChildHeight = dslv.getChildHeight(mSecondPos, item, false);
                            mSecondStartBlank = (float)(item.Height - mSecondChildHeight);
                        }
                        blank = Math.Max((int)(f * mSecondStartBlank), 1);
                        lp = item.LayoutParameters;
                        lp.Height = mSecondChildHeight + blank;
                        item.LayoutParameters = lp;
                    }
                }
            }

            public override void OnStop()
            {
                dslv.doRemoveItem();
            }
        }

        public void removeItem(int which)
        {

            mUseRemoveVelocity = false;
            removeItem(which, 0);
        }

        /**
         * Removes an item from the list and animates the removal.
         *
         * @param which Position to remove (NOTE: headers/footers ignored!
         * this is a position in your input ListAdapter).
         * @param velocityX 
         */
        public void removeItem(int which, float velocityX)
        {
            if (mDragState == IDLE || mDragState == DRAGGING)
            {

                if (mDragState == IDLE)
                {
                    // called from outside drag-sort
                    mSrcPos = HeaderViewsCount + which;
                    mFirstExpPos = mSrcPos;
                    mSecondExpPos = mSrcPos;
                    mFloatPos = mSrcPos;
                    View v = GetChildAt(mSrcPos - FirstVisiblePosition);
                    if (v != null)
                    {
                        v.Visibility = ViewStates.Invisible;
                    }
                }

                mDragState = REMOVING;
                mRemoveVelocityX = velocityX;

                if (mInTouchEvent)
                {
                    switch (mCancelMethod)
                    {
                        case ON_TOUCH_EVENT:
                            base.OnTouchEvent(mCancelEvent);
                            break;
                        case ON_INTERCEPT_TOUCH_EVENT:
                            base.OnInterceptTouchEvent(mCancelEvent);
                            break;
                    }
                }

                if (mRemoveAnimator != null)
                {
                    mRemoveAnimator.start();
                }
                else
                {
                    doRemoveItem(which);
                }
            }
        }

        /**
         * Move an item, bypassing the drag-sort process. Simply calls
         * through to {@link DropListener#drop(int, int)}.
         * 
         * @param from Position to move (NOTE: headers/footers ignored!
         * this is a position in your input ListAdapter).
         * @param to Target position (NOTE: headers/footers ignored!
         * this is a position in your input ListAdapter).
         */
        public void moveItem(int from, int to)
        {
            if (mDropListener != null)
            {
                int count = getInputAdapter().Count;
                if (from >= 0 && from < count && to >= 0 && to < count)
                {
                    mDropListener.Drop(from, to);
                }
            }
        }

        /**
         * Cancel a drag. Calls {@link #stopDrag(bool, bool)} with
         * <code>true</code> as the first argument.
         */
        public void cancelDrag()
        {
            if (mDragState == DRAGGING)
            {
                mDragScroller.stopScrolling(true);
                destroyFloatView();
                clearPositions();
                adjustAllItems();

                if (mInTouchEvent)
                {
                    mDragState = STOPPED;
                }
                else
                {
                    mDragState = IDLE;
                }
            }
        }

        private void clearPositions()
        {
            mSrcPos = -1;
            mFirstExpPos = -1;
            mSecondExpPos = -1;
            mFloatPos = -1;
        }

        private void dropFloatView()
        {
            // must set to avoid cancelDrag being called from the
            // DataSetObserver
            mDragState = DROPPING;

            if (mDropListener != null && mFloatPos >= 0 && mFloatPos < Count)
            {
                int numHeaders = HeaderViewsCount;
                mDropListener.Drop(mSrcPos - numHeaders, mFloatPos - numHeaders);
            }

            destroyFloatView();

            adjustOnReorder();
            clearPositions();
            adjustAllItems();

            // now the drag is done
            if (mInTouchEvent)
            {
                mDragState = STOPPED;
            }
            else
            {
                mDragState = IDLE;
            }
        }

        private void doRemoveItem()
        {
            doRemoveItem(mSrcPos - HeaderViewsCount);
        }

        /**
         * Removes dragged item from the list. Calls RemoveListener.
         */
        private void doRemoveItem(int which)
        {
            // must set to avoid cancelDrag being called from the
            // DataSetObserver
            mDragState = REMOVING;

            // end it
            if (mRemoveListener != null)
            {
                mRemoveListener.Remove(which);
            }

            destroyFloatView();

            adjustOnReorder();
            clearPositions();

            // now the drag is done
            if (mInTouchEvent)
            {
                mDragState = STOPPED;
            }
            else
            {
                mDragState = IDLE;
            }
        }

        private void adjustOnReorder()
        {
            int firstPos = FirstVisiblePosition;
            // Log.d("mobeta", "first="+firstPos+" src="+mSrcPos);
            if (mSrcPos < firstPos)
            {
                // collapsed src item is off screen;
                // adjust the scroll after item heights have been fixed
                View v = GetChildAt(0);
                int top = 0;
                if (v != null)
                {
                    top = v.Top;
                }
                // Log.d("mobeta", "top="+top+" fvh="+mFloatViewHeight);
                SetSelectionFromTop(firstPos - 1, top - PaddingTop);
            }
        }

        /**
         * Stop a drag in progress. Pass <code>true</code> if you would
         * like to remove the dragged item from the list.
         *
         * @param remove Remove the dragged item from the list. Calls
         * a registered RemoveListener, if one exists. Otherwise, calls
         * the DropListener, if one exists.
         *
         * @return True if the stop was successful. False if there is
         * no floating View.
         */
        public bool stopDrag(bool remove)
        {
            mUseRemoveVelocity = false;
            return stopDrag(remove, 0);
        }

        public bool stopDragWithVelocity(bool remove, float velocityX)
        {

            mUseRemoveVelocity = true;
            return stopDrag(remove, velocityX);
        }

        public bool stopDrag(bool remove, float velocityX)
        {
            if (mFloatView != null)
            {
                mDragScroller.stopScrolling(true);

                if (remove)
                {
                    removeItem(mSrcPos - HeaderViewsCount, velocityX);
                }
                else
                {
                    if (mDropAnimator != null)
                    {
                        mDropAnimator.start();
                    }
                    else
                    {
                        dropFloatView();
                    }
                }

                if (mTrackDragSort)
                {
                    mDragSortTracker.stopTracking();
                }

                return true;
            }
            else
            {
                // stop failed
                return false;
            }
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (mIgnoreTouchEvent)
            {
                mIgnoreTouchEvent = false;
                return false;
            }

            if (!mDragEnabled)
            {
                return base.OnTouchEvent(ev);
            }

            bool more = false;

            bool lastCallWasIntercept = mLastCallWasIntercept;
            mLastCallWasIntercept = false;

            if (!lastCallWasIntercept)
            {
                saveTouchCoords(ev);
            }

            // if (mFloatView != null) {
            if (mDragState == DRAGGING)
            {
                onDragTouchEvent(ev);
                more = true; // give us more!
            }
            else
            {
                // what if float view is null b/c we dropped in middle
                // of drag touch event?

                // if (mDragState != STOPPED) {
                if (mDragState == IDLE)
                {
                    if (base.OnTouchEvent(ev))
                    {
                        more = true;
                    }
                }

                MotionEventActions action = ev.Action & MotionEventActions.Mask;

                switch (action)
                {
                    case MotionEventActions.Cancel:
                    case MotionEventActions.Up:
                        doActionUpOrCancel();
                        break;
                    default:
                        if (more)
                        {
                            mCancelMethod = ON_TOUCH_EVENT;
                        }
                        break;
                }
            }

            return more;
        }

        private void doActionUpOrCancel()
        {
            mCancelMethod = NO_CANCEL;
            mInTouchEvent = false;
            if (mDragState == STOPPED)
            {
                mDragState = IDLE;
            }
            mCurrFloatAlpha = mFloatAlpha;
            mListViewIntercepted = false;
            mChildHeightCache.clear();
        }

        private void saveTouchCoords(MotionEvent ev)
        {
            MotionEventActions action = ev.Action & MotionEventActions.Mask;
            if (action != MotionEventActions.Down)
            {
                mLastX = mX;
                mLastY = mY;
            }
            mX = (int)ev.GetX();
            mY = (int)ev.GetY();
            if (action == MotionEventActions.Down)
            {
                mLastX = mX;
                mLastY = mY;
            }
            mOffsetX = (int)ev.RawX - mX;
            mOffsetY = (int)ev.RawY - mY;
        }

        public bool listViewIntercepted()
        {
            return mListViewIntercepted;
        }

        private bool mListViewIntercepted = false;

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (!mDragEnabled)
            {
                return base.OnInterceptTouchEvent(ev);
            }

            saveTouchCoords(ev);
            mLastCallWasIntercept = true;

            MotionEventActions action = ev.Action & MotionEventActions.Mask;

            if (action == MotionEventActions.Down)
            {
                if (mDragState != IDLE)
                {
                    // intercept and ignore
                    mIgnoreTouchEvent = true;
                    return true;
                }
                mInTouchEvent = true;
            }

            bool intercept = false;

            // the following deals with calls to super.onInterceptTouchEvent
            if (mFloatView != null)
            {
                // super's touch event canceled in startDrag
                intercept = true;
            }
            else
            {
                if (base.OnInterceptTouchEvent(ev))
                {
                    mListViewIntercepted = true;
                    intercept = true;
                }

                switch (action)
                {
                    case MotionEventActions.Cancel:
                    case MotionEventActions.Up:
                        doActionUpOrCancel();
                        break;
                    default:
                        if (intercept)
                        {
                            mCancelMethod = ON_TOUCH_EVENT;
                        }
                        else
                        {
                            mCancelMethod = ON_INTERCEPT_TOUCH_EVENT;
                        }
                        break;
                }
            }

            if (action == MotionEventActions.Up || action == MotionEventActions.Cancel)
            {
                mInTouchEvent = false;
            }

            return intercept;
        }

        /**
         * Set the width of each drag scroll region by specifying
         * a fraction of the ListView height.
         *
         * @param heightFraction Fraction of ListView height. Capped at
         * 0.5f.
         * 
         */
        public void setDragScrollStart(float heightFraction)
        {
            setDragScrollStarts(heightFraction, heightFraction);
        }

        /**
         * Set the width of each drag scroll region by specifying
         * a fraction of the ListView height.
         *
         * @param upperFrac Fraction of ListView height for up-scroll bound.
         * Capped at 0.5f.
         * @param lowerFrac Fraction of ListView height for down-scroll bound.
         * Capped at 0.5f.
         * 
         */
        public void setDragScrollStarts(float upperFrac, float lowerFrac)
        {
            if (lowerFrac > 0.5f)
            {
                mDragDownScrollStartFrac = 0.5f;
            }
            else
            {
                mDragDownScrollStartFrac = lowerFrac;
            }

            if (upperFrac > 0.5f)
            {
                mDragUpScrollStartFrac = 0.5f;
            }
            else
            {
                mDragUpScrollStartFrac = upperFrac;
            }

            if (Height != 0)
            {
                updateScrollStarts();
            }
        }

        private void continueDrag(int x, int y)
        {

            // proposed position
            mFloatLoc.X = x - mDragDeltaX;
            mFloatLoc.Y = y - mDragDeltaY;

            doDragFloatView(true);

            int minY = Math.Min(y, mFloatViewMid + mFloatViewHeightHalf);
            int maxY = Math.Max(y, mFloatViewMid - mFloatViewHeightHalf);

            // get the current scroll direction
            int currentScrollDir = mDragScroller.getScrollDir();

            if (minY > mLastY && minY > mDownScrollStartY && currentScrollDir != DragScroller.DOWN)
            {
                // dragged down, it is below the down scroll start and it is not
                // scrolling up

                if (currentScrollDir != DragScroller.STOP)
                {
                    // moved directly from up scroll to down scroll
                    mDragScroller.stopScrolling(true);
                }

                // start scrolling down
                mDragScroller.startScrolling(DragScroller.DOWN);
            }
            else if (maxY < mLastY && maxY < mUpScrollStartY && currentScrollDir != DragScroller.UP)
            {
                // dragged up, it is above the up scroll start and it is not
                // scrolling up

                if (currentScrollDir != DragScroller.STOP)
                {
                    // moved directly from down scroll to up scroll
                    mDragScroller.stopScrolling(true);
                }

                // start scrolling up
                mDragScroller.startScrolling(DragScroller.UP);
            }
            else if (maxY >= mUpScrollStartY && minY <= mDownScrollStartY
                    && mDragScroller.isScrolling())
            {
                // not in the upper nor in the lower drag-scroll regions but it is
                // still scrolling

                mDragScroller.stopScrolling(true);
            }
        }

        private void updateScrollStarts()
        {
            int padTop = PaddingTop;
            int listHeight = Height - padTop - PaddingBottom;
            float heightF = (float)listHeight;

            mUpScrollStartYF = padTop + mDragUpScrollStartFrac * heightF;
            mDownScrollStartYF = padTop + (1.0f - mDragDownScrollStartFrac) * heightF;

            mUpScrollStartY = (int)mUpScrollStartYF;
            mDownScrollStartY = (int)mDownScrollStartYF;

            mDragUpScrollHeight = mUpScrollStartYF - padTop;
            mDragDownScrollHeight = padTop + listHeight - mDownScrollStartYF;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            updateScrollStarts();
        }

        private void adjustAllItems()
        {
            int first = FirstVisiblePosition;
            int last = LastVisiblePosition;

            int begin = Math.Max(0, HeaderViewsCount - first);
            int end = Math.Min(last - first, Count - 1 - FooterViewsCount - first);

            for (int i = begin; i <= end; ++i)
            {
                View v = GetChildAt(i);
                if (v != null)
                {
                    adjustItem(first + i, v, false);
                }
            }
        }

        private void adjustItem(int position)
        {
            View v = GetChildAt(position - FirstVisiblePosition);

            if (v != null)
            {
                adjustItem(position, v, false);
            }
        }

        /**
         * Sets layout param height, gravity, and visibility  on
         * wrapped item.
         */
        private void adjustItem(int position, View v, bool invalidChildHeight)
        {

            // Adjust item height
            ViewGroup.LayoutParams lp = v.LayoutParameters;
            int height;
            if (position != mSrcPos && position != mFirstExpPos && position != mSecondExpPos)
            {
                height = ViewGroup.LayoutParams.WrapContent;
            }
            else
            {
                height = calcItemHeight(position, v, invalidChildHeight);
            }

            if (height != lp.Height)
            {
                lp.Height = height;
                v.LayoutParameters = lp;
            }

            // Adjust item gravity
            if (position == mFirstExpPos || position == mSecondExpPos)
            {
                if (position < mSrcPos)
                {
                    ((DragSortItemView)v).setGravity(GravityFlags.Bottom);
                }
                else if (position > mSrcPos)
                {
                    ((DragSortItemView)v).setGravity(GravityFlags.Top);
                }
            }

            // Finally adjust item visibility

            ViewStates oldVis = v.Visibility;
            ViewStates vis = ViewStates.Visible;

            if (position == mSrcPos && mFloatView != null)
            {
                vis = ViewStates.Invisible;
            }

            if (vis != oldVis)
            {
                v.Visibility = vis;
            }
        }

        private int getChildHeight(int position)
        {
            if (position == mSrcPos)
            {
                return 0;
            }

            View v = GetChildAt(position - FirstVisiblePosition);

            if (v != null)
            {
                // item is onscreen, therefore child height is valid,
                // hence the "true"
                return getChildHeight(position, v, false);
            }
            else
            {
                // item is offscreen
                // first check cache for child height at this position
                int childHeight = mChildHeightCache.get(position);
                if (childHeight != -1)
                {
                    // Log.d("mobeta", "found child height in cache!");
                    return childHeight;
                }

                IListAdapter adapter = Adapter;
                int type = adapter.GetItemViewType(position);

                // There might be a better place for checking for the following
                int typeCount = adapter.ViewTypeCount;
                if (typeCount != mSampleViewTypes.Length)
                {
                    mSampleViewTypes = new View[typeCount];
                }

                if (type >= 0)
                {
                    if (mSampleViewTypes[type] == null)
                    {
                        v = adapter.GetView(position, null, this);
                        mSampleViewTypes[type] = v;
                    }
                    else
                    {
                        v = adapter.GetView(position, mSampleViewTypes[type], this);
                    }
                }
                else
                {
                    // type is HEADER_OR_FOOTER or IGNORE
                    v = adapter.GetView(position, null, this);
                }

                // current child height is invalid, hence "true" below
                childHeight = getChildHeight(position, v, true);

                // cache it because this could have been expensive
                mChildHeightCache.add(position, childHeight);

                return childHeight;
            }
        }

        private int getChildHeight(int position, View item, bool invalidChildHeight)
        {
            if (position == mSrcPos)
            {
                return 0;
            }

            View child;
            if (position < HeaderViewsCount || position >= Count - FooterViewsCount)
            {
                child = item;
            }
            else
            {
                child = ((ViewGroup)item).GetChildAt(0);
            }

            ViewGroup.LayoutParams lp = child.LayoutParameters;

            if (lp != null)
            {
                if (lp.Height > 0)
                {
                    return lp.Height;
                }
            }

            int childHeight = child.Height;

            if (childHeight == 0 || invalidChildHeight)
            {
                measureItem(child);
                childHeight = child.MeasuredHeight;
            }

            return childHeight;
        }

        private int calcItemHeight(int position, View item, bool invalidChildHeight)
        {
            return calcItemHeight(position, getChildHeight(position, item, invalidChildHeight));
        }

        private int calcItemHeight(int position, int childHeight)
        {

            int divHeight = DividerHeight;

            bool isSliding = mAnimate && mFirstExpPos != mSecondExpPos;
            int maxNonSrcBlankHeight = mFloatViewHeight - mItemHeightCollapsed;
            int slideHeight = (int)(mSlideFrac * maxNonSrcBlankHeight);

            int height;

            if (position == mSrcPos)
            {
                if (mSrcPos == mFirstExpPos)
                {
                    if (isSliding)
                    {
                        height = slideHeight + mItemHeightCollapsed;
                    }
                    else
                    {
                        height = mFloatViewHeight;
                    }
                }
                else if (mSrcPos == mSecondExpPos)
                {
                    // if gets here, we know an item is sliding
                    height = mFloatViewHeight - slideHeight;
                }
                else
                {
                    height = mItemHeightCollapsed;
                }
            }
            else if (position == mFirstExpPos)
            {
                if (isSliding)
                {
                    height = childHeight + slideHeight;
                }
                else
                {
                    height = childHeight + maxNonSrcBlankHeight;
                }
            }
            else if (position == mSecondExpPos)
            {
                // we know an item is sliding (b/c 2ndPos != 1stPos)
                height = childHeight + maxNonSrcBlankHeight - slideHeight;
            }
            else
            {
                height = childHeight;
            }

            return height;
        }

        public override void RequestLayout()
        {
            if (!mBlockLayoutRequests)
            {
                base.RequestLayout();
            }
        }

        private int adjustScroll(int movePos, View moveItem, int oldFirstExpPos, int oldSecondExpPos)
        {
            int adjust = 0;

            int childHeight = getChildHeight(movePos);

            int moveHeightBefore = moveItem.Height;
            int moveHeightAfter = calcItemHeight(movePos, childHeight);

            int moveBlankBefore = moveHeightBefore;
            int moveBlankAfter = moveHeightAfter;
            if (movePos != mSrcPos)
            {
                moveBlankBefore -= childHeight;
                moveBlankAfter -= childHeight;
            }

            int maxBlank = mFloatViewHeight;
            if (mSrcPos != mFirstExpPos && mSrcPos != mSecondExpPos)
            {
                maxBlank -= mItemHeightCollapsed;
            }

            if (movePos <= oldFirstExpPos)
            {
                if (movePos > mFirstExpPos)
                {
                    adjust += maxBlank - moveBlankAfter;
                }
            }
            else if (movePos == oldSecondExpPos)
            {
                if (movePos <= mFirstExpPos)
                {
                    adjust += moveBlankBefore - maxBlank;
                }
                else if (movePos == mSecondExpPos)
                {
                    adjust += moveHeightBefore - moveHeightAfter;
                }
                else
                {
                    adjust += moveBlankBefore;
                }
            }
            else
            {
                if (movePos <= mFirstExpPos)
                {
                    adjust -= maxBlank;
                }
                else if (movePos == mSecondExpPos)
                {
                    adjust -= moveBlankAfter;
                }
            }

            return adjust;
        }

        private void measureItem(View item)
        {
            ViewGroup.LayoutParams lp = item.LayoutParameters;
            if (lp == null)
            {
                lp = new AbsListView.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
                item.LayoutParameters = lp;
            }
            int wspec = ViewGroup.GetChildMeasureSpec(mWidthMeasureSpec, ListPaddingLeft
                    + ListPaddingRight, lp.Width);
            int hspec;
            if (lp.Height > 0)
            {
                hspec = MeasureSpec.MakeMeasureSpec(lp.Height, MeasureSpecMode.Exactly);
            }
            else
            {
                hspec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            }
            item.Measure(wspec, hspec);
        }

        private void measureFloatView()
        {
            if (mFloatView != null)
            {
                measureItem(mFloatView);
                mFloatViewHeight = mFloatView.MeasuredHeight;
                mFloatViewHeightHalf = mFloatViewHeight / 2;
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            // Log.d("mobeta", "onMeasure called");
            if (mFloatView != null)
            {
                if (mFloatView.IsLayoutRequested)
                {
                    measureFloatView();
                }
                mFloatViewOnMeasured = true; // set to false after layout
            }
            mWidthMeasureSpec = widthMeasureSpec;
        }

        protected override void LayoutChildren()
        {
            base.LayoutChildren();

            if (mFloatView != null)
            {
                if (mFloatView.IsLayoutRequested && !mFloatViewOnMeasured)
                {
                    // Have to measure here when usual android measure
                    // pass is skipped. This happens during a drag-sort
                    // when layoutChildren is called directly.
                    measureFloatView();
                }
                mFloatView.Layout(0, 0, mFloatView.MeasuredWidth, mFloatView.MeasuredHeight);
                mFloatViewOnMeasured = false;
            }
        }
        

        protected bool onDragTouchEvent(MotionEvent ev)
        {
            // we are in a drag
            MotionEventActions action = ev.Action & MotionEventActions.Mask;

            switch (action)
            {
                case MotionEventActions.Cancel:
                    if (mDragState == DRAGGING)
                    {
                        cancelDrag();
                    }
                    doActionUpOrCancel();
                    break;
                case MotionEventActions.Up:
                    // Log.d("mobeta", "calling stopDrag from onDragTouchEvent");
                    if (mDragState == DRAGGING)
                    {
                        stopDrag(false);
                    }
                    doActionUpOrCancel();
                    break;
                case MotionEventActions.Move:
                    continueDrag((int)ev.GetX(), (int)ev.GetY());
                    break;
            }

            return true;
        }

        private bool mFloatViewInvalidated = false;

        private void invalidateFloatView()
        {
            mFloatViewInvalidated = true;
        }

        /**
         * Start a drag of item at <code>position</code> using the
         * registered FloatViewManager. Calls through
         * to {@link #startDrag(int,View,int,int,int)} after obtaining
         * the floating View from the FloatViewManager.
         *
         * @param position Item to drag.
         * @param dragFlags Flags that restrict some movements of the
         * floating View. For example, set <code>dragFlags |= 
         * ~{@link #DRAG_NEG_X}</code> to allow dragging the floating
         * View in all directions except off the screen to the left.
         * @param deltaX Offset in x of the touch coordinate from the
         * left edge of the floating View (i.e. touch-x minus float View
         * left).
         * @param deltaY Offset in y of the touch coordinate from the
         * top edge of the floating View (i.e. touch-y minus float View
         * top).
         *
         * @return True if the drag was started, false otherwise. This
         * <code>startDrag</code> will fail if we are not currently in
         * a touch event, there is no registered FloatViewManager,
         * or the FloatViewManager returns a null View.
         */
        public bool startDrag(int position, int dragFlags, int deltaX, int deltaY)
        {
            if (!mInTouchEvent || mFloatViewManager == null)
            {
                return false;
            }

            View v = mFloatViewManager.OnCreateFloatView(position);

            if (v == null)
            {
                return false;
            }
            else
            {
                return startDrag(position, v, dragFlags, deltaX, deltaY);
            }

        }

        /**
         * Start a drag of item at <code>position</code> without using
         * a FloatViewManager.
         *
         * @param position Item to drag.
         * @param floatView Floating View.
         * @param dragFlags Flags that restrict some movements of the
         * floating View. For example, set <code>dragFlags |= 
         * ~{@link #DRAG_NEG_X}</code> to allow dragging the floating
         * View in all directions except off the screen to the left.
         * @param deltaX Offset in x of the touch coordinate from the
         * left edge of the floating View (i.e. touch-x minus float View
         * left).
         * @param deltaY Offset in y of the touch coordinate from the
         * top edge of the floating View (i.e. touch-y minus float View
         * top).
         *
         * @return True if the drag was started, false otherwise. This
         * <code>startDrag</code> will fail if we are not currently in
         * a touch event, <code>floatView</code> is null, or there is
         * a drag in progress.
         */
        public bool startDrag(int position, View floatView, int dragFlags, int deltaX, int deltaY)
        {
            if (mDragState != IDLE || !mInTouchEvent || mFloatView != null || floatView == null
                    || !mDragEnabled)
            {
                return false;
            }

            if (Parent != null)
            {
                Parent.RequestDisallowInterceptTouchEvent(true);
            }

            int pos = position + HeaderViewsCount;
            mFirstExpPos = pos;
            mSecondExpPos = pos;
            mSrcPos = pos;
            mFloatPos = pos;

            // mDragState = dragType;
            mDragState = DRAGGING;
            mDragFlags = 0;
            mDragFlags |= dragFlags;

            mFloatView = floatView;
            measureFloatView(); // sets mFloatViewHeight

            mDragDeltaX = deltaX;
            mDragDeltaY = deltaY;
            mDragStartY = mY;

            // updateFloatView(mX - mDragDeltaX, mY - mDragDeltaY);
            mFloatLoc.X = mX - mDragDeltaX;
            mFloatLoc.Y = mY - mDragDeltaY;

            // set src item invisible
            View srcItem = GetChildAt(mSrcPos - FirstVisiblePosition);

            if (srcItem != null)
            {
                srcItem.Visibility = ViewStates.Invisible;
            }

            if (mTrackDragSort)
            {
                mDragSortTracker.startTracking();
            }

            // once float view is created, events are no longer passed
            // to ListView
            switch (mCancelMethod)
            {
                case ON_TOUCH_EVENT:
                    base.OnTouchEvent(mCancelEvent);
                    break;
                case ON_INTERCEPT_TOUCH_EVENT:
                    base.OnInterceptTouchEvent(mCancelEvent);
                    break;
            }

            RequestLayout();

            if (mLiftAnimator != null)
            {
                mLiftAnimator.start();
            }

            return true;
        }

        private void doDragFloatView(bool forceInvalidate)
        {
            int movePos = FirstVisiblePosition + ChildCount / 2;
            View moveItem = GetChildAt(ChildCount / 2);

            if (moveItem == null)
            {
                return;
            }

            doDragFloatView(movePos, moveItem, forceInvalidate);
        }

        private void doDragFloatView(int movePos, View moveItem, bool forceInvalidate)
        {
            mBlockLayoutRequests = true;

            updateFloatView();

            int oldFirstExpPos = mFirstExpPos;
            int oldSecondExpPos = mSecondExpPos;

            bool updated = updatePositions();

            if (updated)
            {
                adjustAllItems();
                int scroll = adjustScroll(movePos, moveItem, oldFirstExpPos, oldSecondExpPos);
                // Log.d("mobeta", "  adjust scroll="+scroll);

                SetSelectionFromTop(movePos, moveItem.Top + scroll - PaddingTop);
                LayoutChildren();
            }

            if (updated || forceInvalidate)
            {
                Invalidate();
            }

            mBlockLayoutRequests = false;
        }

        /**
         * Sets float View location based on suggested values and
         * constraints set in mDragFlags.
         */
        private void updateFloatView()
        {

            if (mFloatViewManager != null)
            {
                mTouchLoc.Set(mX, mY);
                mFloatViewManager.OnDragFloatView(mFloatView, mFloatLoc, mTouchLoc);
            }

            int floatX = mFloatLoc.X;
            int floatY = mFloatLoc.Y;

            // restrict x motion
            int padLeft = PaddingLeft;
            if ((mDragFlags & DRAG_POS_X) == 0 && floatX > padLeft)
            {
                mFloatLoc.X = padLeft;
            }
            else if ((mDragFlags & DRAG_NEG_X) == 0 && floatX < padLeft)
            {
                mFloatLoc.X = padLeft;
            }

            // keep floating view from going past bottom of last header view
            int numHeaders = HeaderViewsCount;
            int numFooters = FooterViewsCount;
            int firstPos = FirstVisiblePosition;
            int lastPos = LastVisiblePosition;

            // Log.d("mobeta",
            // "nHead="+numHeaders+" nFoot="+numFooters+" first="+firstPos+" last="+lastPos);
            int topLimit = PaddingTop;
            if (firstPos < numHeaders)
            {
                topLimit = GetChildAt(numHeaders - firstPos - 1).Bottom;
            }
            if ((mDragFlags & DRAG_NEG_Y) == 0)
            {
                if (firstPos <= mSrcPos)
                {
                    topLimit = Math.Max(GetChildAt(mSrcPos - firstPos).Top, topLimit);
                }
            }
            // bottom limit is top of first footer View or
            // bottom of last item in list
            int bottomLimit = Height - PaddingBottom;
            if (lastPos >= Count - numFooters - 1)
            {
                bottomLimit = GetChildAt(Count - numFooters - 1 - firstPos).Bottom;
            }
            if ((mDragFlags & DRAG_POS_Y) == 0)
            {
                if (lastPos >= mSrcPos)
                {
                    bottomLimit = Math.Min(GetChildAt(mSrcPos - firstPos).Bottom, bottomLimit);
                }
            }

            // Log.d("mobeta", "dragView top=" + (y - mDragDeltaY));
            // Log.d("mobeta", "limit=" + limit);
            // Log.d("mobeta", "mDragDeltaY=" + mDragDeltaY);

            if (floatY < topLimit)
            {
                mFloatLoc.Y = topLimit;
            }
            else if (floatY + mFloatViewHeight > bottomLimit)
            {
                mFloatLoc.Y = bottomLimit - mFloatViewHeight;
            }

            // get y-midpoint of floating view (constrained to ListView bounds)
            mFloatViewMid = mFloatLoc.Y + mFloatViewHeightHalf;
        }

        private void destroyFloatView()
        {
            if (mFloatView != null)
            {
                mFloatView.Visibility = ViewStates.Gone;
                if (mFloatViewManager != null)
                {
                    mFloatViewManager.OnDestroyFloatView(mFloatView);
                }
                mFloatView = null;
                Invalidate();
            }
        }



        public void SetFloatViewManager(IFloatViewManager manager)
        {
            mFloatViewManager = manager;
        }
        
        public void SetDragListener(IDragListener l)
        {
            mDragListener = l;
        }

        /**
         * Allows for easy toggling between a DragSortListView
         * and a regular old ListView. If enabled, items are
         * draggable, where the drag init mode determines how
         * items are lifted (see {@link setDragInitMode(int)}).
         * If disabled, items cannot be dragged.
         *
         * @param enabled Set <code>true</code> to enable list
         * item dragging
         */
        public void setDragEnabled(bool enabled)
        {
            mDragEnabled = enabled;
        }

        public bool isDragEnabled()
        {
            return mDragEnabled;
        }

        /**
         * This better reorder your ListAdapter! DragSortListView does not do this
         * for you; doesn't make sense to. Make sure
         * {@link BaseAdapter#notifyDataSetChanged()} or something like it is called
         * in your implementation. Furthermore, if you have a choiceMode other than
         * none and the ListAdapter does not return true for
         * {@link ListAdapter#hasStableIds()}, you will need to call
         * {@link #moveCheckState(int, int)} to move the check boxes along with the
         * list items.
         * 
         * @param l
         */
        public void SetDropListener(IDropListener l)
        {
            mDropListener = l;
        }

        /**
         * Probably a no-brainer, but make sure that your remove listener
         * calls {@link BaseAdapter#notifyDataSetChanged()} or something like it.
         * When an item removal occurs, DragSortListView
         * relies on a redraw of all the items to recover invisible views
         * and such. Strictly speaking, if you remove something, your dataset
         * has changed...
         * 
         * @param l
         */
        public void SetRemoveListener(IRemoveListener l)
        {
            mRemoveListener = l;
        }


        public void setDragSortListener(IDragSortListener l)
        {
            SetDropListener(l);
            SetDragListener(l);
            SetRemoveListener(l);
        }

        /**
         * Completely custom scroll speed profile. Default increases linearly
         * with position and is constant in time. Create your own by implementing
         * {@link DragSortListView.DragScrollProfile}.
         * 
         * @param ssp
         */
        public void setDragScrollProfile(DragScrollProfile ssp)
        {
            if (ssp != null)
            {
                mScrollProfile = ssp;
            }
        }

        /**
         * Use this to move the check state of an item from one position to another
         * in a drop operation. If you have a choiceMode which is not none, this
         * method must be called when the order of items changes in an underlying
         * adapter which does not have stable IDs (see
         * {@link ListAdapter#hasStableIds()}). This is because without IDs, the
         * ListView has no way of knowing which items have moved where, and cannot
         * update the check state accordingly.
         * <p>
         * A word of warning about a "feature" in Android that you may run into when
         * dealing with movable list items: for an adapter that <em>does</em> have
         * stable IDs, ListView will attempt to locate each item based on its ID and
         * move the check state from the item's old position to the new position —
         * which is all fine and good (and removes the need for calling this
         * function), except for the half-baked approach. Apparently to save time in
         * the naive algorithm used, ListView will only search for an ID in the
         * close neighborhood of the old position. If the user moves an item too far
         * (specifically, more than 20 rows away), ListView will give up and just
         * force the item to be unchecked. So if there is a reasonable chance that
         * the user will move items more than 20 rows away from the original
         * position, you may wish to use an adapter with unstable IDs and call this
         * method manually instead.
         * 
         * @param from
         * @param to
         */
        public void moveCheckState(int from, int to)
        {
            // This method runs in O(n log n) time (n being the number of list
            // items). The bottleneck is the call to AbsListView.setItemChecked,
            // which is O(log n) because of the binary search involved in calling
            // SparseBooleanArray.put().
            //
            // To improve on the average time, we minimize the number of calls to
            // setItemChecked by only calling it for items that actually have a
            // changed state. This is achieved by building a list containing the
            // start and end of the "runs" of checked items, and then moving the
            // runs. Note that moving an item from A to B is essentially a rotation
            // of the range of items in [A, B]. Let's say we have
            // . . U V X Y Z . .
            // and move U after Z. This is equivalent to a rotation one step to the
            // left within the range you are moving across:
            // . . V X Y Z U . .
            //
            // So, to perform the move we enumerate all the runs within the move
            // range, then rotate each run one step to the left or right (depending
            // on move direction). For example, in the list:
            // X X . X X X . X
            // we have two runs. One begins at the last item of the list and wraps
            // around to the beginning, ending at position 1. The second begins at
            // position 3 and ends at position 5. To rotate a run, regardless of
            // length, we only need to set a check mark at one end of the run, and
            // clear a check mark at the other end:
            // X . X X X . X X
            SparseBooleanArray cip = CheckedItemPositions;
            int rangeStart = from;
            int rangeEnd = to;
            if (to < from)
            {
                rangeStart = to;
                rangeEnd = from;
            }
            rangeEnd += 1;

            int[] runStart = new int[cip.Size()];
            int[] runEnd = new int[cip.Size()];
            int runCount = buildRunList(cip, rangeStart, rangeEnd, runStart, runEnd);
            if (runCount == 1 && (runStart[0] == runEnd[0]))
            {
                // Special case where all items are checked, we can never set any
                // item to false like we do below.
                return;
            }

            if (from < to)
            {
                for (int i = 0; i != runCount; i++)
                {
                    SetItemChecked(rotate(runStart[i], -1, rangeStart, rangeEnd), true);
                    SetItemChecked(rotate(runEnd[i], -1, rangeStart, rangeEnd), false);
                }

            }
            else
            {
                for (int i = 0; i != runCount; i++)
                {
                    SetItemChecked(runStart[i], false);
                    SetItemChecked(runEnd[i], true);
                }
            }
        }

        /**
         * Use this when an item has been deleted, to move the check state of all
         * following items up one step. If you have a choiceMode which is not none,
         * this method must be called when the order of items changes in an
         * underlying adapter which does not have stable IDs (see
         * {@link ListAdapter#hasStableIds()}). This is because without IDs, the
         * ListView has no way of knowing which items have moved where, and cannot
         * update the check state accordingly.
         * 
         * See also further comments on {@link #moveCheckState(int, int)}.
         * 
         * @param position
         */
        public void removeCheckState(int position)
        {
            SparseBooleanArray cip = CheckedItemPositions;

            if (cip.Size() == 0)
                return;
            int[] runStart = new int[cip.Size()];
            int[] runEnd = new int[cip.Size()];
            int rangeStart = position;
            int rangeEnd = cip.KeyAt(cip.Size() - 1) + 1;
            int runCount = buildRunList(cip, rangeStart, rangeEnd, runStart, runEnd);
            for (int i = 0; i != runCount; i++)
            {
                if (!(runStart[i] == position || (runEnd[i] < runStart[i] && runEnd[i] > position)))
                {
                    // Only set a new check mark in front of this run if it does
                    // not contain the deleted position. If it does, we only need
                    // to make it one check mark shorter at the end.
                    SetItemChecked(rotate(runStart[i], -1, rangeStart, rangeEnd), true);
                }
                SetItemChecked(rotate(runEnd[i], -1, rangeStart, rangeEnd), false);
            }
        }

        private static int buildRunList(SparseBooleanArray cip, int rangeStart,
                int rangeEnd, int[] runStart, int[] runEnd)
        {
            int runCount = 0;

            int i = findFirstSetIndex(cip, rangeStart, rangeEnd);
            if (i == -1)
                return 0;

            int position = cip.KeyAt(i);
            int currentRunStart = position;
            int currentRunEnd = currentRunStart + 1;
            for (i++; i < cip.Size() && (position = cip.KeyAt(i)) < rangeEnd; i++)
            {
                if (!cip.ValueAt(i)) // not checked => not interesting
                    continue;
                if (position == currentRunEnd)
                {
                    currentRunEnd++;
                }
                else
                {
                    runStart[runCount] = currentRunStart;
                    runEnd[runCount] = currentRunEnd;
                    runCount++;
                    currentRunStart = position;
                    currentRunEnd = position + 1;
                }
            }

            if (currentRunEnd == rangeEnd)
            {
                // rangeStart and rangeEnd are equivalent positions so to be
                // consistent we translate them to the same integer value. That way
                // we can check whether a run covers the entire range by just
                // checking if the start equals the end position.
                currentRunEnd = rangeStart;
            }
            runStart[runCount] = currentRunStart;
            runEnd[runCount] = currentRunEnd;
            runCount++;

            if (runCount > 1)
            {
                if (runStart[0] == rangeStart && runEnd[runCount - 1] == rangeStart)
                {
                    // The last run ends at the end of the range, and the first run
                    // starts at the beginning of the range. So they are actually
                    // part of the same run, except they wrap around the end of the
                    // range. To avoid adjacent runs, we need to merge them.
                    runStart[0] = runStart[runCount - 1];
                    runCount--;
                }
            }
            return runCount;
        }

        private static int rotate(int value, int offset, int lowerBound, int upperBound)
        {
            int windowSize = upperBound - lowerBound;

            value += offset;
            if (value < lowerBound)
            {
                value += windowSize;
            }
            else if (value >= upperBound)
            {
                value -= windowSize;
            }
            return value;
        }

        private static int findFirstSetIndex(SparseBooleanArray sba, int rangeStart, int rangeEnd)
        {
            int size = sba.Size();
            int i = insertionIndexForKey(sba, rangeStart);
            while (i < size && sba.KeyAt(i) < rangeEnd && !sba.ValueAt(i))
                i++;
            if (i == size || sba.KeyAt(i) >= rangeEnd)
                return -1;
            return i;
        }

        private static int insertionIndexForKey(SparseBooleanArray sba, int key)
        {
            int low = 0;
            int high = sba.Size();
            while (high - low > 0)
            {
                int middle = (low + high) >> 1;
                if (sba.KeyAt(middle) < key)
                    low = middle + 1;
                else
                    high = middle;
            }
            return low;
        }


        private class DragScroller : Java.Lang.Object, Java.Lang.IRunnable
        {
            private DragSortListView dslv;
            private bool mAbort;

            private long mPrevTime;
            private long mCurrTime;

            private int dy;
            private float dt;
            private long tStart;
            private int scrollDir;

            public static int STOP = -1;
            public static int UP = 0;
            public static int DOWN = 1;

            private float mScrollSpeed; // pixels per ms

            private bool mScrolling = false;

            private int mLastHeader;
            private int mFirstFooter;

            public bool isScrolling()
            {
                return mScrolling;
            }

            public int getScrollDir()
            {
                return mScrolling ? scrollDir : STOP;
            }

            public DragScroller(DragSortListView dslv)
            {
                this.dslv = dslv;
            }

            public void startScrolling(int dir)
            {
                if (!mScrolling)
                {
                    // Debug.startMethodTracing("dslv-scroll");
                    mAbort = false;
                    mScrolling = true;
                    tStart = SystemClock.UptimeMillis();
                    mPrevTime = tStart;
                    scrollDir = dir;
                    dslv.Post(this);
                }
            }

            public void stopScrolling(bool now)
            {
                if (now)
                {
                    dslv.RemoveCallbacks(this);
                    mScrolling = false;
                }
                else
                {
                    mAbort = true;
                }

                // Debug.stopMethodTracing();
            }

            public void Run()
            {
                if (mAbort)
                {
                    mScrolling = false;
                    return;
                }

                // Log.d("mobeta", "scroll");

                int first = dslv.FirstVisiblePosition;
                int last = dslv.LastVisiblePosition;
                int count = dslv.Count;
                int padTop = dslv.PaddingTop;
                int listHeight = dslv.Height - padTop - dslv.PaddingBottom;

                int minY = Math.Min(dslv.mY, dslv.mFloatViewMid + dslv.mFloatViewHeightHalf);
                int maxY = Math.Max(dslv.mY, dslv.mFloatViewMid - dslv.mFloatViewHeightHalf);

                if (scrollDir == UP)
                {
                    View v = dslv.GetChildAt(0);
                    // Log.d("mobeta", "vtop="+v.getTop()+" padtop="+padTop);
                    if (v == null)
                    {
                        mScrolling = false;
                        return;
                    }
                    else
                    {
                        if (first == 0 && v.Top == padTop)
                        {
                            mScrolling = false;
                            return;
                        }
                    }
                    mScrollSpeed = dslv.mScrollProfile.GetSpeed((dslv.mUpScrollStartYF - maxY)
                            / dslv.mDragUpScrollHeight, mPrevTime);
                }
                else
                {
                    View v = dslv.GetChildAt(last - first);
                    if (v == null)
                    {
                        mScrolling = false;
                        return;
                    }
                    else
                    {
                        if (last == count - 1 && v.Bottom <= listHeight + padTop)
                        {
                            mScrolling = false;
                            return;
                        }
                    }
                    mScrollSpeed = -dslv.mScrollProfile.GetSpeed((minY - dslv.mDownScrollStartYF)
                            / dslv.mDragDownScrollHeight, mPrevTime);
                }

                mCurrTime = SystemClock.UptimeMillis();
                dt = (float)(mCurrTime - mPrevTime);

                // dy is change in View position of a list item; i.e. positive dy
                // means user is scrolling up (list item moves down the screen,
                // remember
                // y=0 is at top of View).
                dy = (int)Math.Round(mScrollSpeed * dt);

                int movePos;
                if (dy >= 0)
                {
                    dy = Math.Min(listHeight, dy);
                    movePos = first;
                }
                else
                {
                    dy = Math.Max(-listHeight, dy);
                    movePos = last;
                }

                View moveItem = dslv.GetChildAt(movePos - first);
                int top = moveItem.Top + dy;

                if (movePos == 0 && top > padTop)
                {
                    top = padTop;
                }

                // always do scroll
                dslv.mBlockLayoutRequests = true;

                dslv.SetSelectionFromTop(movePos, top - padTop);
                dslv.LayoutChildren();
                dslv.Invalidate();

                dslv.mBlockLayoutRequests = false;

                // scroll means relative float View movement
                dslv.doDragFloatView(movePos, moveItem, false);

                mPrevTime = mCurrTime;
                // Log.d("mobeta", "  updated prevTime="+mPrevTime);

                dslv.Post(this);
            }
        }

        private class DragSortTracker
        {
            StringBuilder mBuilder = new StringBuilder();
            private DragSortListView dslv;
            File mFile;

            private int mNumInBuffer = 0;
            private int mNumFlushes = 0;

            private bool mTracking = false;

            public DragSortTracker(DragSortListView dslv)
            {
                this.dslv = dslv;
                File root = Android.OS.Environment.ExternalStorageDirectory;
                mFile = new File(root, "dslv_state.txt");

                if (!mFile.Exists())
                {
                    try
                    {
                        mFile.CreateNewFile();
                        Log.Debug("mobeta", "file created");
                    }
                    catch (IOException e)
                    {
                        Log.Warn("mobeta", "Could not create dslv_state.txt");
                        Log.Debug("mobeta", e.Message);
                    }
                }

            }

            public void startTracking()
            {
                mBuilder.Append("<DSLVStates>\n");
                mNumFlushes = 0;
                mTracking = true;
            }

            public void appendState()
            {
                if (!mTracking)
                {
                    return;
                }

                mBuilder.Append("<DSLVState>\n");
                int children = dslv.ChildCount;
                int first = dslv.FirstVisiblePosition;
                mBuilder.Append("    <Positions>");
                for (int i = 0; i < children; ++i)
                {
                    mBuilder.Append(first + i).Append(",");
                }
                mBuilder.Append("</Positions>\n");

                mBuilder.Append("    <Tops>");
                for (int i = 0; i < children; ++i)
                {
                    mBuilder.Append(dslv.GetChildAt(i).Top).Append(",");
                }
                mBuilder.Append("</Tops>\n");
                mBuilder.Append("    <Bottoms>");
                for (int i = 0; i < children; ++i)
                {
                    mBuilder.Append(dslv.GetChildAt(i).Bottom).Append(",");
                }
                mBuilder.Append("</Bottoms>\n");

                mBuilder.Append("    <FirstExpPos>").Append(dslv.mFirstExpPos).Append("</FirstExpPos>\n");
                mBuilder.Append("    <FirstExpBlankHeight>")
                        .Append(dslv.getItemHeight(dslv.mFirstExpPos) - dslv.getChildHeight(dslv.mFirstExpPos))
                        .Append("</FirstExpBlankHeight>\n");
                mBuilder.Append("    <SecondExpPos>").Append(dslv.mSecondExpPos).Append("</SecondExpPos>\n");
                mBuilder.Append("    <SecondExpBlankHeight>")
                        .Append(dslv.getItemHeight(dslv.mSecondExpPos) - dslv.getChildHeight(dslv.mSecondExpPos))
                        .Append("</SecondExpBlankHeight>\n");
                mBuilder.Append("    <SrcPos>").Append(dslv.mSrcPos).Append("</SrcPos>\n");
                mBuilder.Append("    <SrcHeight>").Append(dslv.mFloatViewHeight + dslv.DividerHeight)
                        .Append("</SrcHeight>\n");
                mBuilder.Append("    <ViewHeight>").Append(dslv.Height).Append("</ViewHeight>\n");
                mBuilder.Append("    <LastY>").Append(dslv.mLastY).Append("</LastY>\n");
                mBuilder.Append("    <FloatY>").Append(dslv.mFloatViewMid).Append("</FloatY>\n");
                mBuilder.Append("    <ShuffleEdges>");
                for (int i = 0; i < children; ++i)
                {
                    mBuilder.Append(dslv.getShuffleEdge(first + i, dslv.GetChildAt(i).Top)).Append(",");
                }
                mBuilder.Append("</ShuffleEdges>\n");

                mBuilder.Append("</DSLVState>\n");
                mNumInBuffer++;

                if (mNumInBuffer > 1000)
                {
                    flush();
                    mNumInBuffer = 0;
                }
            }

            public void flush()
            {
                if (!mTracking)
                {
                    return;
                }

                // save to file on sdcard
                try
                {
                    bool append = true;
                    if (mNumFlushes == 0)
                    {
                        append = false;
                    }
                    FileWriter writer = new FileWriter(mFile, append);

                    writer.Write(mBuilder.ToString());
                    mBuilder.Remove(0, mBuilder.Length);

                    writer.Flush();
                    writer.Close();

                    mNumFlushes++;
                }
                catch (IOException e)
                {
                    // do nothing
                }
            }

            public void stopTracking()
            {
                if (mTracking)
                {
                    mBuilder.Append("</DSLVStates>\n");
                    flush();
                    mTracking = false;
                }
            }

        }
    }
}