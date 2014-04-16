using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DragSortListview
{
    public class DragSortController : SimpleFloatViewManager, View.IOnTouchListener, GestureDetector.IOnGestureListener
    {

        /**
         * Drag init mode enum.
         */
        public static  int ON_DOWN = 0;
        public static  int ON_DRAG = 1;
        public static  int ON_LONG_PRESS = 2;

        private int mDragInitMode = ON_DOWN;

        private bool mSortEnabled = true;

        /**
         * Remove mode enum.
         */
        public static  int CLICK_REMOVE = 0;
        public static  int FLING_REMOVE = 1;

        /**
         * The current remove mode.
         */
        private int mRemoveMode;

        private bool mRemoveEnabled = false;
        private bool mIsRemoving = false;

        private GestureDetector mDetector;

        private GestureDetector mFlingRemoveDetector;

        private int mTouchSlop;

        public static  int MISS = -1;

        private int mHitPos = MISS;
        private int mFlingHitPos = MISS;

        private int mClickRemoveHitPos = MISS;

        private int[] mTempLoc = new int[2];

        private int mItemX;
        private int mItemY;

        private int mCurrX;
        private int mCurrY;

        private bool mDragging = false;

        private float mFlingSpeed = 500f;

        private int mDragHandleId;

        private int mClickRemoveId;

        private int mFlingHandleId;
        private bool mCanDrag;

        private DragSortListView mDslv;
        private int mPositionX;

        private GestureDetector.IOnGestureListener mFlingRemoveListener;

        /**
         * Calls {@link #DragSortController(DragSortListView, int)} with a
         * 0 drag handle id, FLING_RIGHT_REMOVE remove mode,
         * and ON_DOWN drag init. By default, sorting is enabled, and
         * removal is disabled.
         *
         * @param dslv The DSLV instance
         */
        public DragSortController(DragSortListView dslv)
            : this(dslv, 0, ON_DOWN, FLING_REMOVE)
        {
        }

        public DragSortController(DragSortListView dslv, int dragHandleId, int dragInitMode, int removeMode)
            : this(dslv, dragHandleId, dragInitMode, removeMode, 0)
        {
        }

        public DragSortController(DragSortListView dslv, int dragHandleId, int dragInitMode, int removeMode, int clickRemoveId)
            : this(dslv, dragHandleId, dragInitMode, removeMode, clickRemoveId, 0)
        {
        }

        /**
         * By default, sorting is enabled, and removal is disabled.
         *
         * @param dslv The DSLV instance
         * @param dragHandleId The resource id of the View that represents
         * the drag handle in a list item.
         */
        public DragSortController(DragSortListView dslv, int dragHandleId, int dragInitMode,
                int removeMode, int clickRemoveId, int flingHandleId)
            : base(dslv)
        {
            mDslv = dslv;
            mDetector = new GestureDetector(dslv.Context, this);
            mFlingRemoveListener = new MySimpleOnGestureListener(mRemoveEnabled, mIsRemoving, mDslv, mPositionX, mFlingSpeed);
            mFlingRemoveDetector = new GestureDetector(dslv.Context, mFlingRemoveListener);
            mFlingRemoveDetector.IsLongpressEnabled = false;
            mTouchSlop = ViewConfiguration.Get(dslv.Context).ScaledTouchSlop;
            mDragHandleId = dragHandleId;
            mClickRemoveId = clickRemoveId;
            mFlingHandleId = flingHandleId;
            setRemoveMode(removeMode);
            setDragInitMode(dragInitMode);
        }


        public int getDragInitMode()
        {
            return mDragInitMode;
        }

        /**
         * Set how a drag is initiated. Needs to be one of
         * {@link ON_DOWN}, {@link ON_DRAG}, or {@link ON_LONG_PRESS}.
         *
         * @param mode The drag init mode.
         */
        public void setDragInitMode(int mode)
        {
            mDragInitMode = mode;
        }

        /**
         * Enable/Disable list item sorting. Disabling is useful if only item
         * removal is desired. Prevents drags in the vertical direction.
         *
         * @param enabled Set <code>true</code> to enable list
         * item sorting.
         */
        public void setSortEnabled(bool enabled)
        {
            mSortEnabled = enabled;
        }

        public bool isSortEnabled()
        {
            return mSortEnabled;
        }

        /**
         * One of {@link CLICK_REMOVE}, {@link FLING_RIGHT_REMOVE},
         * {@link FLING_LEFT_REMOVE},
         * {@link SLIDE_RIGHT_REMOVE}, or {@link SLIDE_LEFT_REMOVE}.
         */
        public void setRemoveMode(int mode)
        {
            mRemoveMode = mode;
        }

        public int getRemoveMode()
        {
            return mRemoveMode;
        }

        /**
         * Enable/Disable item removal without affecting remove mode.
         */
        public void setRemoveEnabled(bool enabled)
        {
            mRemoveEnabled = enabled;
        }

        public bool isRemoveEnabled()
        {
            return mRemoveEnabled;
        }

        /**
         * Set the resource id for the View that represents the drag
         * handle in a list item.
         *
         * @param id An android resource id.
         */
        public void setDragHandleId(int id)
        {
            mDragHandleId = id;
        }

        /**
         * Set the resource id for the View that represents the fling
         * handle in a list item.
         *
         * @param id An android resource id.
         */
        public void setFlingHandleId(int id)
        {
            mFlingHandleId = id;
        }

        /**
         * Set the resource id for the View that represents click
         * removal button.
         *
         * @param id An android resource id.
         */
        public void setClickRemoveId(int id)
        {
            mClickRemoveId = id;
        }

        /**
         * Sets flags to restrict certain motions of the floating View
         * based on DragSortController settings (such as remove mode).
         * Starts the drag on the DragSortListView.
         *
         * @param position The list item position (includes headers).
         * @param deltaX Touch x-coord minus left edge of floating View.
         * @param deltaY Touch y-coord minus top edge of floating View.
         *
         * @return True if drag started, false otherwise.
         */
        public bool startDrag(int position, int deltaX, int deltaY)
        {

            int dragFlags = 0;
            if (mSortEnabled && !mIsRemoving)
            {
                dragFlags |= DragSortListView.DRAG_POS_Y | DragSortListView.DRAG_NEG_Y;
            }
            if (mRemoveEnabled && mIsRemoving)
            {
                dragFlags |= DragSortListView.DRAG_POS_X;
                dragFlags |= DragSortListView.DRAG_NEG_X;
            }

            mDragging = mDslv.startDrag(position - mDslv.HeaderViewsCount, dragFlags, deltaX, deltaY);
            return mDragging;
        }

        public bool OnTouch(View v, MotionEvent ev)
        {
            if (!mDslv.isDragEnabled() || mDslv.listViewIntercepted())
            {
                return false;
            }

            mDetector.OnTouchEvent(ev);
            if (mRemoveEnabled && mDragging && mRemoveMode == FLING_REMOVE)
            {
                mFlingRemoveDetector.OnTouchEvent(ev);
            }

            MotionEventActions action = ev.Action & MotionEventActions.Mask;
            switch (action)
            {
                case MotionEventActions.Down:
                    mCurrX = (int)ev.GetX();
                    mCurrY = (int)ev.GetY();
                    break;
                case MotionEventActions.Up:
                    if (mRemoveEnabled && mIsRemoving)
                    {
                        int x = mPositionX >= 0 ? mPositionX : -mPositionX;
                        int removePoint = mDslv.Width / 2;
                        if (x > removePoint)
                        {
                            mDslv.stopDragWithVelocity(true, 0);
                        }
                    }
                    mIsRemoving = false;
                    mDragging = false;
                    break;
                case MotionEventActions.Cancel:
                    mIsRemoving = false;
                    mDragging = false;
                    break;
            }

            return false;
        }

        /**
         * Overrides to provide fading when slide removal is enabled.
         */
        public override void OnDragFloatView(View floatView, Point position, Point touch)
        {
            if (mRemoveEnabled && mIsRemoving)
            {
                mPositionX = position.X;
            }
        }

        /**
         * Get the position to start dragging based on the ACTION_DOWN
         * MotionEvent. This function simply calls
         * {@link #dragHandleHitPosition(MotionEvent)}. Override
         * to change drag handle behavior;
         * this function is called internally when an ACTION_DOWN
         * event is detected.
         *
         * @param ev The ACTION_DOWN MotionEvent.
         *
         * @return The list position to drag if a drag-init gesture is
         * detected; MISS if unsuccessful.
         */
        public virtual int StartDragPosition(MotionEvent ev)
        {
            return dragHandleHitPosition(ev);
        }

        public int startFlingPosition(MotionEvent ev)
        {
            return mRemoveMode == FLING_REMOVE ? flingHandleHitPosition(ev) : MISS;
        }

        /**
         * Checks for the touch of an item's drag handle (specified by
         * {@link #setDragHandleId(int)}), and returns that item's position
         * if a drag handle touch was detected.
         *
         * @param ev The ACTION_DOWN MotionEvent.

         * @return The list position of the item whose drag handle was
         * touched; MISS if unsuccessful.
         */
        public int dragHandleHitPosition(MotionEvent ev)
        {
            return viewIdHitPosition(ev, mDragHandleId);
        }

        public int flingHandleHitPosition(MotionEvent ev)
        {
            return viewIdHitPosition(ev, mFlingHandleId);
        }

        public int viewIdHitPosition(MotionEvent ev, int id)
        {
            int x = (int)ev.GetX();
            int y = (int)ev.GetY();

            int touchPos = mDslv.PointToPosition(x, y); // includes headers/footers

            int numHeaders = mDslv.HeaderViewsCount;
            int numFooters = mDslv.FooterViewsCount;
            int count = mDslv.Count;

            // Log.d("mobeta", "touch down on position " + itemnum);
            // We're only interested if the touch was on an
            // item that's not a header or footer.
            if (touchPos != AdapterView.InvalidPosition && touchPos >= numHeaders
                    && touchPos < (count - numFooters))
            {
                View item = mDslv.GetChildAt(touchPos - mDslv.FirstVisiblePosition);
                int rawX = (int)ev.RawX;
                int rawY = (int)ev.RawY;

                View dragBox = id == 0 ? item : (View)item.FindViewById(id);
                if (dragBox != null)
                {
                    dragBox.GetLocationOnScreen(mTempLoc);

                    if (rawX > mTempLoc[0] && rawY > mTempLoc[1] &&
                            rawX < mTempLoc[0] + dragBox.Width &&
                            rawY < mTempLoc[1] + dragBox.Height)
                    {

                        mItemX = item.Left;
                        mItemY = item.Top;

                        return touchPos;
                    }
                }
            }

            return MISS;
        }

        public bool OnDown(MotionEvent ev)
        {
            if (mRemoveEnabled && mRemoveMode == CLICK_REMOVE)
            {
                mClickRemoveHitPos = viewIdHitPosition(ev, mClickRemoveId);
            }

            mHitPos = StartDragPosition(ev);
            if (mHitPos != MISS && mDragInitMode == ON_DOWN)
            {
                startDrag(mHitPos, (int)ev.GetX() - mItemX, (int)ev.GetY() - mItemY);
            }

            mIsRemoving = false;
            mCanDrag = true;
            mPositionX = 0;
            mFlingHitPos = startFlingPosition(ev);

            return true;
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {

            int x1 = (int)e1.GetX();
            int y1 = (int)e1.GetY();
            int x2 = (int)e2.GetX();
            int y2 = (int)e2.GetY();
            int deltaX = x2 - mItemX;
            int deltaY = y2 - mItemY;

            if (mCanDrag && !mDragging && (mHitPos != MISS || mFlingHitPos != MISS))
            {
                if (mHitPos != MISS)
                {
                    if (mDragInitMode == ON_DRAG && Math.Abs(y2 - y1) > mTouchSlop && mSortEnabled)
                    {
                        startDrag(mHitPos, deltaX, deltaY);
                    }
                    else if (mDragInitMode != ON_DOWN && Math.Abs(x2 - x1) > mTouchSlop && mRemoveEnabled)
                    {
                        mIsRemoving = true;
                        startDrag(mFlingHitPos, deltaX, deltaY);
                    }
                }
                else if (mFlingHitPos != MISS)
                {
                    if (Math.Abs(x2 - x1) > mTouchSlop && mRemoveEnabled)
                    {
                        mIsRemoving = true;
                        startDrag(mFlingHitPos, deltaX, deltaY);
                    }
                    else if (Math.Abs(y2 - y1) > mTouchSlop)
                    {
                        mCanDrag = false; // if started to scroll the list then
                        // don't allow sorting nor fling-removing
                    }
                }
            }
            // return whatever
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
            // Log.d("mobeta", "lift listener long pressed");
            if (mHitPos != MISS && mDragInitMode == ON_LONG_PRESS)
            {
                mDslv.PerformHapticFeedback(FeedbackConstants.LongPress);
                startDrag(mHitPos, mCurrX - mItemX, mCurrY - mItemY);
            }
        }

        // complete the OnGestureListener interface
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        // complete the OnGestureListener interface
        public bool OnSingleTapUp(MotionEvent ev)
        {
            if (mRemoveEnabled && mRemoveMode == CLICK_REMOVE)
            {
                if (mClickRemoveHitPos != MISS)
                {
                    mDslv.removeItem(mClickRemoveHitPos - mDslv.HeaderViewsCount);
                }
            }
            return true;
        }

        // complete the OnGestureListener interface
        public void OnShowPress(MotionEvent ev)
        {
            // do nothing
        }

        private class MySimpleOnGestureListener : GestureDetector.SimpleOnGestureListener
        {
            bool mRemoveEnabled;
            bool mIsRemoving;
            DragSortListView mDslv;
            int mPositionX;
            float mFlingSpeed;
            public MySimpleOnGestureListener(bool mRemoveEnabled, bool mIsRemoving, DragSortListView mDslv, int mPositionX, float mFlingSpeed)
            {
                this.mRemoveEnabled = mRemoveEnabled;
                this.mIsRemoving = mIsRemoving;
                this.mDslv = mDslv;
                this.mPositionX = mPositionX;
                this.mFlingSpeed = mFlingSpeed;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                // Log.d("mobeta", "on fling remove called");
                if (mRemoveEnabled && mIsRemoving)
                {
                    int w = mDslv.Width;
                    int minPos = w / 5;
                    if (velocityX > mFlingSpeed)
                    {
                        if (mPositionX > -minPos)
                        {
                            mDslv.stopDragWithVelocity(true, velocityX);
                        }
                    }
                    else if (velocityX < -mFlingSpeed)
                    {
                        if (mPositionX < minPos)
                        {
                            mDslv.stopDragWithVelocity(true, velocityX);
                        }
                    }
                    mIsRemoving = false;
                }
                return false;
            }
        }
    }
}