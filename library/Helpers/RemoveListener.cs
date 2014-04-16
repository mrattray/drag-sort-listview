using System;
using Android.Widget;
using DragSortListview.Interfaces;

namespace DragSortListview.Helpers
{
    public class RemoveListener : IRemoveListener
    {
        Action<int> remover;
        public RemoveListener(Action<int> remove)
        {
            this.remover = remove;
        }
        public void Remove(int which)
        {
            remover(which);
        }
    }
}