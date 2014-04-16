using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DragSortListview;

namespace SampleDSLV
{
    [Activity(Label = "TestBedDSLV")]
    public class TestBedDSLV : FragmentActivity, RemoveModeDialog.RemoveOkListener, DragInitModeDialog.DragOkListener, EnablesDialog.EnabledOkListener
    {

        private int mNumHeaders = 0;
        private int mNumFooters = 0;

        private int mDragStartMode = DragSortController.ON_DRAG;
        private bool mRemoveEnabled = true;
        private int mRemoveMode = DragSortController.FLING_REMOVE;
        private bool mSortEnabled = true;
        private bool mDragEnabled = true;

        private String mTag = "dslvTag";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.test_bed_main);

            if (savedInstanceState == null)
            {
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.test_bed, getNewDslvFragment(), mTag).Commit();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.mode_menu, menu);
            return true;
        }
        
        public void OnRemoveOkClick(int removeMode)
        {
            
            if (removeMode != mRemoveMode)
            {
                mRemoveMode = removeMode;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.test_bed, getNewDslvFragment(), mTag).Commit();
            }
        }

        public void OnDragOkClick(int dragStartMode)
        {
            mDragStartMode = dragStartMode;
            DSLVFragment f = (DSLVFragment)SupportFragmentManager.FindFragmentByTag(mTag);
            f.getController().setDragInitMode(dragStartMode);
        }

        public void OnEnabledOkClick(bool drag, bool sort, bool remove)
        {
            mSortEnabled = sort;
            mRemoveEnabled = remove;
            mDragEnabled = drag;
            DSLVFragment f = (DSLVFragment)SupportFragmentManager.FindFragmentByTag(mTag);
            DragSortListView dslv = (DragSortListView)f.ListView;
            f.getController().setRemoveEnabled(remove);
            f.getController().setSortEnabled(sort);
            dslv.setDragEnabled(drag);
        }

        private Fragment getNewDslvFragment()
        {
            DSLVFragmentClicks f = DSLVFragmentClicks.newInstance(mNumHeaders, mNumFooters);
            f.removeMode = mRemoveMode;
            f.removeEnabled = mRemoveEnabled;
            f.dragStartMode = mDragStartMode;
            f.sortEnabled = mSortEnabled;
            f.dragEnabled = mDragEnabled;
            return f;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle item selection
            FragmentTransaction transaction;
            DSLVFragment f = (DSLVFragment)SupportFragmentManager.FindFragmentByTag(mTag);
            DragSortListView dslv = (DragSortListView)f.ListView;
            DragSortController control = f.getController();

            switch (item.ItemId)
            {
                case Resource.Id.select_remove_mode:
                    RemoveModeDialog rdialog = new RemoveModeDialog(mRemoveMode);
                    rdialog.setRemoveOkListener(this);
                    rdialog.Show(SupportFragmentManager, "RemoveMode");
                    return true;
                case Resource.Id.select_drag_init_mode:
                    DragInitModeDialog ddialog = new DragInitModeDialog(mDragStartMode);
                    ddialog.setDragOkListener(this);
                    ddialog.Show(SupportFragmentManager, "DragInitMode");
                    return true;
                case Resource.Id.select_enables:
                    EnablesDialog edialog = new EnablesDialog(mDragEnabled, mSortEnabled, mRemoveEnabled);
                    edialog.setEnabledOkListener(this);
                    edialog.Show(SupportFragmentManager, "Enables");
                    return true;
                case Resource.Id.add_header:
                    mNumHeaders++;

                    transaction = SupportFragmentManager.BeginTransaction();
                    transaction.Replace(Resource.Id.test_bed, getNewDslvFragment(), mTag);
                    transaction.SetTransition(FragmentTransaction.TransitFragmentFade);
                    transaction.Commit();
                    return true;
                case Resource.Id.add_footer:
                    mNumFooters++;

                    transaction = SupportFragmentManager.BeginTransaction();
                    transaction.Replace(Resource.Id.test_bed, getNewDslvFragment(), mTag);
                    transaction.SetTransition(FragmentTransaction.TransitFragmentFade);
                    transaction.Commit();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}