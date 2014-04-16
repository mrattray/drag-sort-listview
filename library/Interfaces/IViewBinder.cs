using Android.Database;
using Android.Views;

namespace DragSortListview.Interfaces
{
    /**
    * This class can be used by external clients of SimpleCursorAdapter
    * to bind values fom the Cursor to views.
    *
    * You should use this class to bind values from the Cursor to views
    * that are not directly supported by SimpleCursorAdapter or to
    * change the way binding occurs for views supported by
    * SimpleCursorAdapter.
    *
    * @see SimpleCursorAdapter#bindView(android.view.View, android.content.Context, android.database.Cursor)
    * @see SimpleCursorAdapter#setViewImage(ImageView, String) 
    * @see SimpleCursorAdapter#setViewText(TextView, String)
    */
    public interface IViewBinder
    {
        /**
         * Binds the Cursor column defined by the specified index to the specified view.
         *
         * When binding is handled by this ViewBinder, this method must return true.
         * If this method returns false, SimpleCursorAdapter will attempts to handle
         * the binding on its own.
         *
         * @param view the view to bind the data to
         * @param cursor the cursor to get the data from
         * @param columnIndex the column at which the data can be found in the cursor
         *
         * @return true if the data was bound to the view, false otherwise
         */
        bool SetViewValue(View view, ICursor cursor, int columnIndex);
    }
}