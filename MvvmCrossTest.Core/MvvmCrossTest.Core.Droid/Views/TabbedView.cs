using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;
using MvvmCrossTest.Core.Droid.Views.Fragment;
using System.Collections.Generic;

namespace MvvmCrossTest.Core.Droid.Views
{
    [Activity(Label = "View for TabbedView")]
    public class TabbedView : BaseView
    {
        protected override int LayoutResource => Resource.Layout.TabbedView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            //Tabbed Setup
            var viewPager = FindViewById<ViewPager>(Resource.Id.main_view_pager);
            var fragments = new List<MvxCachingFragmentStatePagerAdapter.FragmentInfo>();

            TabbedViewModel vm = ViewModel as TabbedViewModel;
            foreach (var myViewModel in vm.Tabs)
                fragments.Add(new MvxCachingFragmentStatePagerAdapter.FragmentInfo(myViewModel.TabTitle, typeof(TabbedChildViewFragment), myViewModel));

            viewPager.Adapter = new MvxCachingFragmentStatePagerAdapter(this, SupportFragmentManager, fragments);

            //If you want to start at specific tab
            //viewPager.SetCurrentItem(ViewModel.CurrentPage, false);

            var tabLayout = FindViewById<TabLayout>(Resource.Id.main_tablayout);
            tabLayout.SetupWithViewPager(viewPager);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}
