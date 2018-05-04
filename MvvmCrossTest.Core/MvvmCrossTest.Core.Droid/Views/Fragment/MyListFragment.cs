using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;

namespace MvvmCrossTest.Core.Droid.Views.Fragment
{
    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.fragment.MyListFragment")]
    public class MyListFragment : MvxFragment<MyListViewModel>
    {
        public MyListFragment()
        {
            this.RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.MyListFragment, null);
        }

        public override string UniqueImmutableCacheTag => "MyListFragment";
    }
}