using System;
using DragSortListview.Interfaces;

namespace DragSortListview.Helpers
{
    public class DropListener : IDropListener
    {
        Action<int, int> dropper;
        public DropListener(Action<int, int> drop)
        {
            this.dropper = drop;
        }

        public virtual void Drop(int from, int to)
        {
            dropper(from, to);
        }
    }
}