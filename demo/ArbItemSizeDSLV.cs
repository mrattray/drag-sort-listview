using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using DragSortListview;
using DragSortListview.Interfaces;

namespace SampleDSLV
{
    [Activity(Label = "ArbItemSizeDSLV")]
    public class ArbItemSizeDSLV : ListActivity
    {

        private JazzAdapter adapter;

        private List<JazzArtist> mArtists;

        private String[] mArtistNames;
        private String[] mArtistAlbums;

        /** Called when the activity is first created. */

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hetero_main);

            DragSortListView lv = (DragSortListView)ListView;


            mArtistNames = Resources.GetStringArray(Resource.Array.jazz_artist_names);
            mArtistAlbums = Resources.GetStringArray(
                    Resource.Array.jazz_artist_albums);

            mArtists = new List<JazzArtist>();
            JazzArtist ja;
            for (int i = 0; i < mArtistNames.Length; ++i)
            {
                ja = new JazzArtist();
                ja.name = mArtistNames[i];
                if (i < mArtistAlbums.Length)
                {
                    ja.albums = mArtistAlbums[i];
                }
                else
                {
                    ja.albums = "No albums listed";
                }
                mArtists.Add(ja);
            }

            adapter = new JazzAdapter(this, mArtists);

            lv.SetDropListener(new DropListener(adapter));
            lv.SetRemoveListener(new RemoveListener(adapter));
            ListAdapter = adapter;
        }

        private class DropListener : IDropListener
        {
            JazzAdapter adapter;
            public DropListener(JazzAdapter adapter)
            {
                this.adapter = adapter;
            }

            public void Drop(int from, int to)
            {
                JazzArtist item = (JazzArtist)adapter.GetItem(from);

                adapter.Remove(item);
                adapter.Insert(item, to);
            }
        }

        private class RemoveListener : IRemoveListener
        {
            JazzAdapter adapter;
            public RemoveListener(JazzAdapter adapter)
            {
                this.adapter = adapter;
            }
            public void Remove(int which)
            {
                adapter.Remove(adapter.GetItem(which));
            }
        }

        private class JazzArtist : Java.Lang.Object
        {
            public String name;
            public String albums;

            public override String ToString()
            {
                return name;
            }
        }

        private class ViewHolder : Java.Lang.Object
        {
            public TextView albumsView;
        }

        private class JazzAdapter : ArrayAdapter<JazzArtist>
        {
            public JazzAdapter(Context context, List<JazzArtist> artists)
                : base(context, Resource.Layout.jazz_artist_list_item, Resource.Id.artist_name_textview, artists)
            {
            }

            public View GetView(int position, View convertView, ViewGroup parent)
            {
                View v = base.GetView(position, convertView, parent);

                if (v != convertView && v != null)
                {
                    ViewHolder holder = new ViewHolder();

                    TextView tv = (TextView)v
                            .FindViewById(Resource.Id.artist_albums_textview);
                    holder.albumsView = tv;

                    v.Tag = holder;
                }

                ViewHolder vHolder = (ViewHolder)v.Tag;
                String albums = GetItem(position).albums;

                vHolder.albumsView.SetText(albums, TextView.BufferType.Normal);

                return v;
            }
        }
    }
}