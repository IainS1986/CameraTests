using Android.App;
using Android.Graphics;
using Android.Hardware;
using Android.Opengl;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCrossTest.Core.Droid.Controls;

namespace MvvmCrossTest.Core.Droid.Views
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TestView : MvxAppCompatActivity
    {
        protected int LayoutResource => Resource.Layout.TestView;

        public Android.Hardware.Camera m_camera;
        public AutoFitTextureView m_preview;
        public CameraPreviewCallback m_previewCallback;
        private byte[] m_buffer;

        private Paint m_paint;
        private GLSurfaceView m_glView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            m_glView = new TextureGLSurfaceView(this);
            //m_glView.SetRenderer(new SimpleGLRenderer());
            SetContentView(m_glView);
        }
    }
}
