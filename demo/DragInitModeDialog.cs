using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using DragSortListview;

namespace SampleDSLV
{
    /**
     * Sets drag init mode on DSLV controller passed into ctor.
     */
    public class DragInitModeDialog : DialogFragment
    {
        private DragSortController mControl;

        private int mDragInitMode;

        private DragOkListener mListener;

        public DragInitModeDialog()
            : base()
        {
            mDragInitMode = DragSortController.ON_DOWN;
        }

        public DragInitModeDialog(int dragStartMode)
            : base()
        {
            mDragInitMode = dragStartMode;
        }

        public interface DragOkListener
        {
            void OnDragOkClick(int removeMode);
        }

        public void setDragOkListener(DragOkListener l)
        {
            mListener = l;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Set the dialog title
            builder.SetTitle(Resource.String.select_remove_mode)
                    .SetSingleChoiceItems(Resource.Array.drag_init_mode_labels, mDragInitMode, SingleChoiceClick)
                // Set the action buttons
                    .SetPositiveButton(Resource.String.ok, PositiveClick)
                    .SetNegativeButton(Resource.String.cancel, NegativeClick);
            return builder.Create();
        }

        private void SingleChoiceClick(object sender, DialogClickEventArgs e)
        {
            mDragInitMode = e.Which;
        }

        private void PositiveClick(object sender, DialogClickEventArgs e)
        {
            if (mListener != null)
            {
                mListener.OnDragOkClick(mDragInitMode);
            }
        }

        private void NegativeClick(object sender, DialogClickEventArgs e)
        {
        }
    }
}
