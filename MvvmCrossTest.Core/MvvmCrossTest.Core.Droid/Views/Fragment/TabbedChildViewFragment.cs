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
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;

namespace MvvmCrossTest.Core.Droid.Views.Fragment
{
    [Register("mvvmcrosstest.core.droid.views.fragment.TabbedChildViewFragment")]
    class TabbedChildViewFragment : MvxFragment<TabbedChildViewModel>
    {
        public TabbedChildViewFragment()
        {
            this.RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.TabbedChildViewFragment, null);
        }
    }
}