using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;

namespace SampleDSLV
{
    public class EnablesDialog : DialogFragment
    {

        private bool[] mEnabled;

        private EnabledOkListener mListener;

        public EnablesDialog()
            : base()
        {
            mEnabled = new bool[3];
            mEnabled[0] = true;
            mEnabled[1] = true;
            mEnabled[2] = false;
        }

        public EnablesDialog(bool drag, bool sort, bool remove)
            : base()
        {
            mEnabled = new bool[3];
            mEnabled[0] = drag;
            mEnabled[1] = sort;
            mEnabled[2] = remove;
        }

        public interface EnabledOkListener
        {
            void OnEnabledOkClick(bool drag, bool sort, bool remove);
        }

        public void setEnabledOkListener(EnabledOkListener l)
        {
            mListener = l;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Set the dialog title
            builder.SetTitle(Resource.String.select_remove_mode)
                    .SetMultiChoiceItems(Resource.Array.enables_labels, mEnabled, MultiChoiceClick)
                // Set the action buttons
                    .SetPositiveButton(Resource.String.ok, PositiveClick)
                    .SetNegativeButton(Resource.String.cancel, NegativeClick);

            return builder.Create();
        }

        private void MultiChoiceClick(object sender, DialogMultiChoiceClickEventArgs e)
        {
            mEnabled[e.Which] = e.IsChecked;
        }

        private void PositiveClick(object sender, DialogClickEventArgs e)
        {
            if (mListener != null)
            {
                mListener.OnEnabledOkClick(mEnabled[0], mEnabled[1], mEnabled[2]);
            }
        }

        private void NegativeClick(object sender, DialogClickEventArgs e)
        {
        }
    }
}