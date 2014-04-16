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

namespace DragSortListview.Interfaces
{
    /**
     * Interface for customization of the floating View appearance
     * and dragging behavior. Implement
     * your own and pass it to {@link #setFloatViewManager}. If
     * your own is not passed, the default {@link SimpleFloatViewManager}
     * implementation is used.
     */
    public interface IFloatViewManager
    {
        /**
         * Return the floating View for item at <code>position</code>.
         * DragSortListView will measure and layout this View for you,
         * so feel free to just inflate it. You can help DSLV by
         * setting some {@link ViewGroup.LayoutParams} on this View;
         * otherwise it will set some for you (with a width of FILL_PARENT
         * and a height of WRAP_CONTENT).
         *
         * @param position Position of item to drag (NOTE:
         * <code>position</code> excludes header Views; thus, if you
         * want to call {@link ListView#getChildAt(int)}, you will need
         * to add {@link ListView#getHeaderViewsCount()} to the index).
         *
         * @return The View you wish to display as the floating View.
         */
        View OnCreateFloatView(int position);

        /**
         * Called whenever the floating View is dragged. Float View
         * properties can be changed here. Also, the upcoming location
         * of the float View can be altered by setting
         * <code>location.x</code> and <code>location.y</code>.
         *
         * @param floatView The floating View.
         * @param location The location (top-left; relative to DSLV
         * top-left) at which the float
         * View would like to appear, given the current touch location
         * and the offset provided in {@link DragSortListView#startDrag}.
         * @param touch The current touch location (relative to DSLV
         * top-left).
         * @param pendingScroll 
         */
        void OnDragFloatView(View floatView, Point location, Point touch);

        /**
         * Called when the float View is dropped; lets you perform
         * any necessary cleanup. The internal DSLV floating View
         * reference is set to null immediately after this is called.
         *
         * @param floatView The floating View passed to
         * {@link #onCreateFloatView(int)}.
         */
        void OnDestroyFloatView(View floatView);
    }
}