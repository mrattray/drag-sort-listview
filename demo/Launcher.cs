using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;

namespace SampleDSLV
{
    [Activity(Label = "SampleDSLV", MainLauncher = true, Icon = "@drawable/icon")]
    public class Launcher : ListActivity
    {
        //private ArrayAdapter<ActivityInfo> adapter;
        private MyAdapter adapter;

        private List<ActivityInfo> mActivities = null;

        private String[] mActTitles;
        private String[] mActDescs;

        /** Called when the activity is first created. */
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.launcher);

            try
            {
                PackageInfo pi = PackageManager.GetPackageInfo(
                  "com.SampleDSLV", PackageInfoFlags.Activities);

                mActivities = new List<ActivityInfo>(pi.Activities);
                String ourName = Class.Name;
                for (int i = 0; i < mActivities.Count; ++i)
                {
                    if (ourName.Equals(mActivities[i].Name))
                    {
                        mActivities.RemoveAt(i);
                        break;
                    }
                }
            }
            catch (PackageManager.NameNotFoundException e)
            {
                // Do nothing. Adapter will be empty.
            }

            mActTitles = Resources.GetStringArray(Resource.Array.activity_titles);
            mActDescs = Resources.GetStringArray(Resource.Array.activity_descs);

            //adapter = new ArrayAdapter<ActivityInfo>(this,
            //  R.layout.launcher_item, R.id.text, mActivities);
            adapter = new MyAdapter(this);

            ListAdapter = adapter;
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            Type actType = typeof(TestBedDSLV);
            switch (position)
            {
                case 0:
                    actType = typeof(TestBedDSLV);
                    break;
                case 1:
                    actType = typeof(ArbItemSizeDSLV);
                    break;
                case 2:
                    actType = typeof(WarpDSLV);
                    break;
                case 3:
                    actType = typeof(BGHandle);
                    break;
                case 4:
                    actType = typeof(Sections);
                    break;
                case 5:
                    actType = typeof(CursorDSLV);
                    break;
                case 6:
                    actType = typeof(MultipleChoiceListView);
                    break;
                case 7:
                    actType = typeof(SingleChoiceListView);
                    break;
            }
            Intent intent = new Intent(this, actType);
            StartActivity(intent);
        }

        private class MyAdapter : ArrayAdapter<ActivityInfo>
        {
            Launcher context;
            public MyAdapter(Launcher context)
                : base(context, Resource.Layout.launcher_item, Resource.Id.activity_title, context.mActivities)
            {
                this.context = context;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            { 
                View v = base.GetView(position, convertView, parent);

                TextView title = (TextView)v.FindViewById(Resource.Id.activity_title);
                TextView desc = (TextView)v.FindViewById(Resource.Id.activity_desc);
                try
                {
                    title.SetText(context.mActTitles[position], TextView.BufferType.Normal);
                    desc.SetText(context.mActDescs[position], TextView.BufferType.Normal);
                }
                catch
                {
                    title.Text = "added one";
                }
                return v;
            }
        }
    }
}

