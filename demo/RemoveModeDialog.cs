using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using DragSortListview;

namespace SampleDSLV
{
    /**
     * Simply passes remove mode back to OnOkListener
     */
    public class RemoveModeDialog : DialogFragment
    {

        private int mRemoveMode;

        private RemoveOkListener mListener;

        public RemoveModeDialog()
            : base()
        {
            mRemoveMode = DragSortController.FLING_REMOVE;
        }

        public RemoveModeDialog(int inRemoveMode)
            : base()
        {
            mRemoveMode = inRemoveMode;
        }

        public interface RemoveOkListener
        {
            void OnRemoveOkClick(int removeMode);
        }

        public void setRemoveOkListener(RemoveOkListener l)
        {
            mListener = l;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Set the dialog title
            builder.SetTitle(Resource.String.select_remove_mode)
                    .SetSingleChoiceItems(Resource.Array.remove_mode_labels, mRemoveMode, SingleChoiceClick)
                // Set the action buttons
                    .SetPositiveButton(Resource.String.ok, PositiveClick)
                    .SetNegativeButton(Resource.String.cancel, NegativeClick);

            return builder.Create();
        }


        private void SingleChoiceClick(object sender, DialogClickEventArgs e)
        {
            mRemoveMode = e.Which;
        }

        private void PositiveClick(object sender, DialogClickEventArgs e)
        {
            if (mListener != null)
            {
                mListener.OnRemoveOkClick(mRemoveMode);
            }
        }

        private void NegativeClick(object sender, DialogClickEventArgs e)
        {
        }
    }
}