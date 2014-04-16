using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DragSortListview.Interfaces;

namespace DragSortListview
{
    /**
 * A subclass of {@link android.widget.CursorAdapter} that provides
 * reordering of the elements in the Cursor based on completed
 * drag-sort operations. The reordering is a simple mapping of
 * list positions into Cursor positions (the Cursor is unchanged).
 * To persist changes made by drag-sorts, one can retrieve the
 * mapping with the {@link #getCursorPositions()} method, which
 * returns the reordered list of Cursor positions.
 *
 * An instance of this class is passed
 * to {@link DragSortListView#setAdapter(ListAdapter)} and, since
 * this class implements the {@link DragSortListView.DragSortListener}
 * interface, it is automatically set as the DragSortListener for
 * the DragSortListView instance.
 */
    public abstract class DragSortCursorAdapter : CursorAdapter, IDragSortListener
    {

        public static int REMOVED = -1;

        /**
         * Key is ListView position, value is Cursor position
         */
        private SparseIntArray mListMapping = new SparseIntArray();

        private List<int> mRemovedCursorPositions = new List<int>();

        public DragSortCursorAdapter(Context context, ICursor c)
            : base(context, c)
        {
        }

        public DragSortCursorAdapter(Context context, ICursor c, bool autoRequery)
            : base(context, c, autoRequery)
        {
        }

        public DragSortCursorAdapter(Context context, ICursor c, int flags)
            : base(context, c)
        {
        }


        /**
         * Swaps Cursor and clears list-Cursor mapping.
         *
         * @see android.widget.CursorAdapter#swapCursor(android.database.Cursor)
         */
        public virtual ICursor SwapCursor(ICursor newCursor)
        {
            ICursor old = base.Cursor;
            base.ChangeCursor(newCursor);
            resetMappings();
            return old;
        }


        /**
         * Changes Cursor and clears list-Cursor mapping.
         *
         * @see android.widget.CursorAdapter#changeCursor(android.database.Cursor)
         */
        public override void ChangeCursor(ICursor newCursor)
        {
            base.ChangeCursor(newCursor);
            resetMappings();
        }

        /**
         * Resets list-cursor mapping.
         */
        public void reset()
        {
            resetMappings();
            NotifyDataSetChanged();
        }

        private void resetMappings()
        {
            mListMapping.Clear();
            mRemovedCursorPositions.Clear();
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return base.GetItem(mListMapping.Get(position, position));
        }

        public override long GetItemId(int position)
        {
            return base.GetItemId(mListMapping.Get(position, position));
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return base.GetDropDownView(mListMapping.Get(position, position), convertView, parent);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return base.GetView(mListMapping.Get(position, position), convertView, parent);
        }

        /**
         * On drop, this updates the mapping between Cursor positions
         * and ListView positions. The Cursor is unchanged. Retrieve
         * the current mapping with {@link getCursorPositions()}.
         *
         * @see DragSortListView.DropListener#drop(int, int)
         */
        public void Drop(int from, int to)
        {
            if (from != to)
            {
                int cursorFrom = mListMapping.Get(from, from);

                if (from > to)
                {
                    for (int i = from; i > to; --i)
                    {
                        mListMapping.Put(i, mListMapping.Get(i - 1, i - 1));
                    }
                }
                else
                {
                    for (int i = from; i < to; ++i)
                    {
                        mListMapping.Put(i, mListMapping.Get(i + 1, i + 1));
                    }
                }
                mListMapping.Put(to, cursorFrom);

                cleanMapping();
                NotifyDataSetChanged();
            }
        }

        /**
         * On remove, this updates the mapping between Cursor positions
         * and ListView positions. The Cursor is unchanged. Retrieve
         * the current mapping with {@link getCursorPositions()}.
         *
         * @see DragSortListView.RemoveListener#remove(int)
         */
        public void Remove(int which)
        {
            int cursorPos = mListMapping.Get(which, which);
            if (!mRemovedCursorPositions.Contains(cursorPos))
            {
                mRemovedCursorPositions.Add(cursorPos);
            }

            int newCount = Count;
            for (int i = which; i < newCount; ++i)
            {
                mListMapping.Put(i, mListMapping.Get(i + 1, i + 1));
            }

            mListMapping.Delete(newCount);

            cleanMapping();
            NotifyDataSetChanged();
        }

        /**
         * Does nothing. Just completes DragSortListener interface.
         */
        public void Drag(int from, int to)
        {
            // do nothing
        }

        /**
         * Remove unnecessary mappings from sparse array.
         */
        private void cleanMapping()
        {
            List<int> toRemove = new List<int>();

            int size = mListMapping.Size();
            for (int i = 0; i < size; ++i)
            {
                if (mListMapping.KeyAt(i) == mListMapping.ValueAt(i))
                {
                    toRemove.Add(mListMapping.KeyAt(i));
                }
            }

            size = toRemove.Count();
            for (int i = 0; i < size; ++i)
            {
                mListMapping.Delete(toRemove[i]);
            }
        }
        public override int Count
        {
            get
            {
                return base.Count - mRemovedCursorPositions.Count();
            }
        }

        /**
         * Get the Cursor position mapped to by the provided list position
         * (given all previously handled drag-sort
         * operations).
         *
         * @param position List position
         *
         * @return The mapped-to Cursor position
         */
        public int getCursorPosition(int position)
        {
            return mListMapping.Get(position, position);
        }

        /**
         * Get the current order of Cursor positions presented by the
         * list.
         */
        public List<int> getCursorPositions()
        {
            List<int> result = new List<int>();

            for (int i = 0; i < Count; ++i)
            {
                result.Add(mListMapping.Get(i, i));
            }

            return result;
        }

        /**
         * Get the list position mapped to by the provided Cursor position.
         * If the provided Cursor position has been removed by a drag-sort,
         * this returns {@link #REMOVED}.
         *
         * @param cursorPosition A Cursor position
         * @return The mapped-to list position or REMOVED
         */
        public int getListPosition(int cursorPosition)
        {
            if (mRemovedCursorPositions.Contains(cursorPosition))
            {
                return REMOVED;
            }

            int index = mListMapping.IndexOfValue(cursorPosition);
            if (index < 0)
            {
                return cursorPosition;
            }
            else
            {
                return mListMapping.KeyAt(index);
            }
        }
    }
}