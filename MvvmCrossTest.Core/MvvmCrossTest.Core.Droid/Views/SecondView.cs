using Android.App;
using Android.OS;
using Android.Views;

namespace MvvmCrossTest.Core.Droid.Views
{
    [Activity(Label = "View for SecondViewModel")]
    public class SecondView : BaseView
    {
        protected override int LayoutResource => Resource.Layout.SecondView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
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
