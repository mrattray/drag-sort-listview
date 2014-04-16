using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using DragSortListview.Interfaces;

namespace DragSortListview
{
    /**
     * Simple implementation of the FloatViewManager class. Uses list
     * items as they appear in the ListView to create the floating View.
     */
    public class SimpleFloatViewManager : Java.Lang.Object, IFloatViewManager
    {

        private Bitmap mFloatBitmap;

        private ImageView mImageView;

        private Color mFloatBGColor = Color.Black;

        private ListView mListView;

        public SimpleFloatViewManager(ListView lv)
        {
            mListView = lv;
        }

        public void setBackgroundColor(Color color)
        {
            mFloatBGColor = color;
        }

        /**
         * This simple implementation creates a Bitmap copy of the
         * list item currently shown at ListView <code>position</code>.
         */
        public virtual View OnCreateFloatView(int position)
        {
            // Guaranteed that this will not be null? I think so. Nope, got
            // a NullPointerException once...
            View v = mListView.GetChildAt(position + mListView.HeaderViewsCount - mListView.FirstVisiblePosition);

            if (v == null)
            {
                return null;
            }

            v.Pressed = false;

            // Create a copy of the drawing cache so that it does not get
            // recycled by the framework when the list tries to clean up memory
            //v.setDrawingCacheQuality(View.DRAWING_CACHE_QUALITY_HIGH);
            v.DrawingCacheEnabled = true;
            mFloatBitmap = Bitmap.CreateBitmap(v.GetDrawingCache(false));
            v.DrawingCacheEnabled = false;

            if (mImageView == null)
            {
                mImageView = new ImageView(mListView.Context);
            }
            mImageView.SetBackgroundColor(mFloatBGColor);
            mImageView.SetPadding(0, 0, 0, 0);
            mImageView.SetImageBitmap(mFloatBitmap);
            mImageView.LayoutParameters = new ViewGroup.LayoutParams(v.Width, v.Height);

            return mImageView;
        }

        /**
         * This does nothing
         */
        public virtual void OnDragFloatView(View floatView, Point position, Point touch)
        {
            // do nothing
        }

        /**
         * Removes the Bitmap from the ImageView created in
         * onCreateFloatView() and tells the system to recycle it.
         */
        public virtual void OnDestroyFloatView(View floatView)
        {
            ((ImageView)floatView).SetImageDrawable(null);

            mFloatBitmap.Recycle();
            mFloatBitmap = null;
        }

    }
}
