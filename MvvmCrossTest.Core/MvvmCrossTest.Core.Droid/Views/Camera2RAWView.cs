using Android.App;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;
using MvvmCrossTest.Core.Droid.Controls;
using MvvmCrossTest.Core.Droid.Services;
using System;
using Camera = Android.Hardware.Camera;

//https://github.com/xamarin/monodroid-samples/tree/master/android5.0/Camera2Raw
namespace MvvmCrossTest.Core.Droid.Views
{
    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.Camera2RAWView")]
    public class Camera2RAWView : MvxFragment<Camera2RAWViewModel>
    {
        public override string UniqueImmutableCacheTag => "Camera2RAWView";

        public DroidCameraService m_droidCameraService;
        public AutoFitTextureView m_preview;

        public Camera2RAWView()
        {
            this.RetainInstance = true;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            m_droidCameraService = new DroidCameraService(Activity, FragmentManager);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Camera2RAWView, null);
        }

        public override void OnStart()
        {
            base.OnStart();

            m_preview = View.FindViewById<AutoFitTextureView>(Resource.Id.preview);
            m_droidCameraService.OnViewCreated(Activity, FragmentManager, m_preview);
        }

        public override void OnResume()
        {
            base.OnResume();
            m_droidCameraService.OnResume();
        }

        public override void OnPause()
        {
            m_droidCameraService.OnPause();
            base.OnPause();
        }
    }
}
