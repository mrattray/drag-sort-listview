using Android.Database;
using Android.Runtime;

namespace DragSortListview.Interfaces
{

    /**
     * This class can be used by external clients of SimpleCursorAdapter
     * to define how the Cursor should be converted to a String.
     *
     * @see android.widget.CursorAdapter#convertToString(android.database.Cursor)
     */
    public interface ICursorToStringConverter
    {
        /**
         * Returns a CharSequence representing the specified Cursor.
         *
         * @param cursor the cursor for which a CharSequence representation
         *        is requested
         *
         * @return a non-null CharSequence representing the cursor
         */
        CharSequence convertToString(ICursor cursor);
    }

}