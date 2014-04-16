using Android.Content;
using Android.Util;
using Android.Widget;

namespace SampleDSLV
{
    public class CheckableLinearLayout : LinearLayout, ICheckable
    {
        private const int CHECKABLE_CHILD_INDEX = 1;
        private ICheckable child;

        public CheckableLinearLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {

        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            child = (ICheckable)GetChildAt(CHECKABLE_CHILD_INDEX);
        }

        public bool Checked
        {
            get
            {
                return child.Checked;
            }
            set
            {
                child.Checked = value;
            }
        }

        public void Toggle()
        {
            child.Toggle();
        }
    }
}