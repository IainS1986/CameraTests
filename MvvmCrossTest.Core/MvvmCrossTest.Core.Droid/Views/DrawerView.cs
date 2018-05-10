using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCrossTest.Core.Core.ViewModels;
using MvvmCrossTest.Core.Droid.Views.Fragment;
using System.Linq;

namespace MvvmCrossTest.Core.Droid.Views
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DrawerView : MvxCachingFragmentCompatActivity<DrawerViewModel>
    {
        protected int LayoutResource => Resource.Layout.DrawerView;

        ActionBarDrawerToggle _drawerToggle;

        //Drawer List View
        ListView _drawerListView;

        //Drawer Content View
        DrawerLayout _drawerLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(LayoutResource);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            // Set the padding to match the Status Bar height
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            toolbar.SetPadding(0, GetStatusBarHeight(), 0, 0);

            //Window.AddFlags(WindowManagerFlags.TranslucentStatus);

            _drawerListView = FindViewById<ListView>(Resource.Id.drawerListView);
            _drawerListView.ItemClick += (s, e) => ShowFragmentAt(e.Position);
            _drawerListView.Adapter = new ArrayAdapter<string>(
                this,
                global::Android.Resource.Layout.SimpleListItem1,
                ViewModel.MenuItems.ToArray());

            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);

            _drawerToggle = new ActionBarDrawerToggle(
                this,
                _drawerLayout,
                Resource.String.OpenDrawerString,
                Resource.String.CloseDrawerString);
            _drawerToggle.DrawerIndicatorEnabled = true;

            _drawerLayout.SetDrawerListener(_drawerToggle);

            ShowFragmentAt(0);
        }

        void ShowFragmentAt(int position)
        {
            ViewModel.NavigateTo(position);

            Title = ViewModel.MenuItems.ElementAt(position);

            _drawerLayout.CloseDrawer(_drawerListView);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            _drawerToggle.SyncState();

            base.OnPostCreate(savedInstanceState);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (_drawerToggle.OnOptionsItemSelected(item))
                return true;

            return base.OnOptionsItemSelected(item);
        }

        public int GetStatusBarHeight()
        {
            int result = 0;
            int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                result = Resources.GetDimensionPixelSize(resourceId);
            }
            return result;
        }
    }
}
