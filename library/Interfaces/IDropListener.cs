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

namespace DragSortListview.Interfaces
{
    /**
     * Your implementation of this has to reorder your ListAdapter! 
     * Make sure to call
     * {@link BaseAdapter#notifyDataSetChanged()} or something like it
     * in your implementation.
     * 
     * @author heycosmo
     *
     */
    public interface IDropListener
    {
        void Drop(int from, int to);
    }

}