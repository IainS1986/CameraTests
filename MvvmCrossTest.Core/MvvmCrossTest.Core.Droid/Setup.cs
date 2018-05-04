using Android.Content;
using MvvmCross.Binding.Combiners;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Presenter;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Platform;
using MvvmCrossTest.Core.Core.Combiners;
using MvvmCrossTest.Core.Core.Converters;

namespace MvvmCrossTest.Core.Droid
{
    public class Setup : MvxAppCompatSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override void InitializeBindingBuilder()
        {
            base.InitializeBindingBuilder();

            IMvxValueCombinerRegistry combinerRegistry = Mvx.Resolve<IMvxValueCombinerRegistry>();
            combinerRegistry.AddOrOverwrite("FullNameMultiBindTest", new FullNameMultiBindTestValueCombiner());
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var mvxFragmentsPresenter = new MvxFragmentsPresenter(AndroidViewAssemblies);
            Mvx.RegisterSingleton<IMvxAndroidViewPresenter>(mvxFragmentsPresenter);
            return mvxFragmentsPresenter;
        }
    }
}
