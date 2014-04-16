using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DragSortListview
{
    // taken from v4 rev. 10 ResourceCursorAdapter.java

    /**
     * Static library support version of the framework's {@link android.widget.ResourceCursorAdapter}.
     * Used to write apps that run on platforms prior to Android 3.0.  When running
     * on Android 3.0 or above, this implementation is still used; it does not try
     * to switch to the framework's implementation.  See the framework SDK
     * documentation for a class overview.
     */
    public abstract class ResourceDragSortCursorAdapter : DragSortCursorAdapter
    {
        private int mLayout;

        private int mDropDownLayout;

        private LayoutInflater mInflater;

        /**
         * Constructor the enables auto-requery.
         *
         * @deprecated This option is discouraged, as it results in Cursor queries
         * being performed on the application's UI thread and thus can cause poor
         * responsiveness or even Application Not Responding errors.  As an alternative,
         * use {@link android.app.LoaderManager} with a {@link android.content.CursorLoader}.
         *
         * @param context The context where the ListView associated with this adapter is running
         * @param layout resource identifier of a layout file that defines the views
         *            for this list item.  Unless you override them later, this will
         *            define both the item views and the drop down views.
         */
        public ResourceDragSortCursorAdapter(Context context, int layout, ICursor c)
            : base(context, c)
        {
            mLayout = mDropDownLayout = layout;
            mInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        /**
         * Constructor with default behavior as per
         * {@link CursorAdapter#CursorAdapter(Context, Cursor, bool)}; it is recommended
         * you not use this, but instead {@link #ResourceCursorAdapter(Context, int, Cursor, int)}.
         * When using this constructor, {@link #FLAG_REGISTER_CONTENT_OBSERVER}
         * will always be set.
         *
         * @param context The context where the ListView associated with this adapter is running
         * @param layout resource identifier of a layout file that defines the views
         *            for this list item.  Unless you override them later, this will
         *            define both the item views and the drop down views.
         * @param c The cursor from which to get the data.
         * @param autoRequery If true the adapter will call requery() on the
         *                    cursor whenever it changes so the most recent
         *                    data is always displayed.  Using true here is discouraged.
         */
        public ResourceDragSortCursorAdapter(Context context, int layout, ICursor c, bool autoRequery)
            : base(context, c, autoRequery)
        {
            mLayout = mDropDownLayout = layout;
            mInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        /**
         * Standard constructor.
         *
         * @param context The context where the ListView associated with this adapter is running
         * @param layout Resource identifier of a layout file that defines the views
         *            for this list item.  Unless you override them later, this will
         *            define both the item views and the drop down views.
         * @param c The cursor from which to get the data.
         * @param flags Flags used to determine the behavior of the adapter,
         * as per {@link CursorAdapter#CursorAdapter(Context, Cursor, int)}.
         */
        public ResourceDragSortCursorAdapter(Context context, int layout, ICursor c, int flags)
            : base(context, c, flags)
        {
            mLayout = mDropDownLayout = layout;
            mInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        /**
         * Inflates view(s) from the specified XML file.
         * 
         * @see android.widget.CursorAdapter#newView(android.content.Context,
         *      android.database.Cursor, ViewGroup)
         */
        public override View NewView(Context context, ICursor cursor, ViewGroup parent)
        {
            return mInflater.Inflate(mLayout, parent, false);
        }

        public override View NewDropDownView(Context context, ICursor cursor, ViewGroup parent)
        {
            return mInflater.Inflate(mDropDownLayout, parent, false);
        }

        /**
         * <p>Sets the layout resource of the item views.</p>
         *
         * @param layout the layout resources used to create item views
         */
        public void setViewResource(int layout)
        {
            mLayout = layout;
        }

        /**
         * <p>Sets the layout resource of the drop down views.</p>
         *
         * @param dropDownLayout the layout resources used to create drop down views
         */
        public void setDropDownViewResource(int dropDownLayout)
        {
            mDropDownLayout = dropDownLayout;
        }
    }
}