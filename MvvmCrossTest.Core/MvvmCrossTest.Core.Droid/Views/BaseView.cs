using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace MvvmCrossTest.Core.Droid.Views
{
    public abstract class BaseView : MvxAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(LayoutResource);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            // Set the padding to match the Status Bar height
            toolbar.SetPadding(0, GetStatusBarHeight(), 0, 0);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        protected abstract int LayoutResource { get; }

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
