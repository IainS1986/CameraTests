using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;
using MvvmCrossTest.Core.Droid.Controls;
using MvvmCrossTest.Core.Droid.Helper;

namespace MvvmCrossTest.Core.Droid.Views
{
    public class BaseCameraView<TViewModel> : MvxFragment<TViewModel>, TextureView.ISurfaceTextureListener where TViewModel : class, IMvxViewModel 
    {
        //TODO MOVE TO FPS CONTROL
        public DateTime LastFrameTime = DateTime.Now;

        public Android.Hardware.Camera m_camera;
        public AutoFitTextureView m_preview;
        public ImageView m_processedPreview;
        public CameraPreviewCallback m_previewCallback;
        public TextView m_fpsDisplay;
        private byte[] m_buffer;

        private Paint m_paint;
        private TextureGLSurfaceView m_glView;

        private int m_frames;
        private DateTime m_lastFrame = DateTime.Now;

        protected virtual bool UseOpenGL { get; set; } = false;
        protected virtual bool ViewRawPreview { get; set; } = true;
        protected virtual bool ViewProcessedPreview { get; set; } = false;
        protected virtual bool ProcessPreview { get; set; } = false;
        protected virtual bool CameraCallback { get; set; } = true;

        //RGB Buffer to reduce allocation
        private int[] RGB;

        public BaseCameraView()
        {
            this.RetainInstance = true;

            m_paint = new Paint();
            m_paint.AntiAlias = true;
            m_paint.FilterBitmap = true;
            m_paint.Dither = true;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.CameraView, null);
        }

        public override void OnPause()
        {
            base.OnPause();

            if (m_glView != null)
                m_glView.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();

            if (m_glView != null)
                m_glView.OnResume();
        }

        public override void OnStart()
        {
            base.OnStart();

            m_fpsDisplay = View.FindViewById<TextView>(Resource.Id.fpsLabel);
            m_fpsDisplay.SetBackgroundColor(Color.Black);
            m_fpsDisplay.SetTextColor(Color.White);

            m_preview = View.FindViewById<AutoFitTextureView>(Resource.Id.preview);
            m_preview.SurfaceTextureListener = this;

            m_processedPreview = View.FindViewById<ImageView>(Resource.Id.processedPreview);
            m_processedPreview.SetBackgroundColor(Color.HotPink);

            if (UseOpenGL)
            {
                LinearLayout insert = View.FindViewById<LinearLayout>(Resource.Id.insert);
                m_glView = new TextureGLSurfaceView(Activity);
                insert.AddView(m_glView);
            }
        }

        private void OnFrame(byte[] data, Android.Hardware.Camera camera)
        {
            var size = camera.GetParameters().PreviewSize;

            if (ProcessPreview)
            {
                //GraphicsHelper.applyGrayScale(RGB, data, size.Width, size.Height);
                GraphicsHelper.applyGrayScaleAndRotate90(RGB, data, size.Width, size.Height);
            }

            if (UseOpenGL)
            {
                Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width, Bitmap.Config.Argb8888);
                m_glView.mRenderer.LoadTexture(bm);
            }
            else if (ViewProcessedPreview)
            {
                Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width, Bitmap.Config.Argb8888);
                m_processedPreview.SetImageBitmap(bm);
            }

            //m_camera.AddCallbackBuffer(m_buffer);

            DateTime now = DateTime.Now;
            if (now - LastFrameTime > TimeSpan.FromMilliseconds(1000))
            {
                m_fpsDisplay.Text = m_frames.ToString();
                LastFrameTime = now;
                m_frames = 0;
            }
            m_frames++;
        }

        #region TextureView.ISurfaceTextureListener

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            m_camera = Android.Hardware.Camera.Open();
            m_previewCallback = new CameraPreviewCallback(OnFrame);

            var format = m_camera.GetParameters().PreviewFormat;
            var size = m_camera.GetParameters().PreviewSize;
            int totalpixels = size.Width * size.Height;
            RGB = new int[totalpixels];

            int totalBytes = (totalpixels * ImageFormat.GetBitsPerPixel(format)) / 8;

            m_buffer = new byte[totalBytes];

            //Hacking just for my moto g4 https://stackoverflow.com/questions/3841122/android-camera-preview-is-sideways
            m_camera.SetDisplayOrientation(90);

            try
            {
                m_camera.SetPreviewTexture(surface);

                if (CameraCallback)
                {
                    m_camera.SetPreviewCallback(m_previewCallback);
                    //m_camera.SetPreviewCallbackWithBuffer(m_previewCallback);
                    //m_camera.AddCallbackBuffer(m_buffer);
                }

                m_camera.StartPreview();

                m_preview.Visibility = (ViewRawPreview || CameraCallback == false || (UseOpenGL == false && ViewProcessedPreview == false)) ? ViewStates.Visible : ViewStates.Gone;
                m_processedPreview.Visibility = (ViewProcessedPreview && CameraCallback) ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (m_camera != null)
            {
                m_camera.StopPreview();
                m_camera.SetPreviewCallback(null);//Thanks android
                m_camera.SetPreviewCallbackWithBuffer(null);//Thanks android
                m_camera.Release();
                m_camera.Dispose();
                m_camera = null;

                m_previewCallback.Dispose();
                m_previewCallback = null;
            }

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        #endregion
    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraToSurfaceTextureView")]
    public class CameraToSurfaceTextureView : BaseCameraView<CameraToSurfaceTextureViewModel>
    {
        public override string UniqueImmutableCacheTag => "CameraToSurfaceTextureView";

        protected override bool UseOpenGL { get; set; } = false;
        protected override bool ViewRawPreview { get; set; } = true;
        protected override bool ViewProcessedPreview { get; set; } = false;
        protected override bool ProcessPreview { get; set; } = false;
        protected override bool CameraCallback { get; set; } = false;
    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraToSurfaceTextureWithCallbackView")]
    public class CameraToSurfaceTextureWithCallbackView : BaseCameraView<CameraToSurfaceTextureWithCallbackViewModel>
    {
        public override string UniqueImmutableCacheTag => "CameraToSurfaceTextureWithCallbackView";

        protected override bool UseOpenGL { get; set; } = false;
        protected override bool ViewRawPreview { get; set; } = true;
        protected override bool ViewProcessedPreview { get; set; } = false;
        protected override bool ProcessPreview { get; set; } = false;
        protected override bool CameraCallback { get; set; } = true;
    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraToSurfaceTextureWithCallbackAndProcessingView")]
    public class CameraToSurfaceTextureWithCallbackAndProcessingView : BaseCameraView<CameraToSurfaceTextureWithCallbackAndProcessingViewModel>
    {
        public override string UniqueImmutableCacheTag => "CameraToSurfaceTextureWithCallbackAndProcessingView";

        protected override bool UseOpenGL { get; set; } = false;
        protected override bool ViewRawPreview { get; set; } = true;
        protected override bool ViewProcessedPreview { get; set; } = false;
        protected override bool ProcessPreview { get; set; } = true;
        protected override bool CameraCallback { get; set; } = true;
    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraToImageViewWithCallbackAndProcessingView")]
    public class CameraToImageViewWithCallbackAndProcessingView : BaseCameraView<CameraToImageViewWithCallbackAndProcessingViewModel>
    {
        public override string UniqueImmutableCacheTag => "CameraToImageViewWithCallbackAndProcessingView";

        protected override bool UseOpenGL { get; set; } = false;
        protected override bool ViewRawPreview { get; set; } = false;
        protected override bool ViewProcessedPreview { get; set; } = true;
        protected override bool ProcessPreview { get; set; } = true;
        protected override bool CameraCallback { get; set; } = true;
    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraToOpenGLWithCallbackAndProcessingView")]
    public class CameraToOpenGLWithCallbackAndProcessingView : BaseCameraView<CameraToOpenGLWithCallbackAndProcessingViewModel>
    {
        public override string UniqueImmutableCacheTag => "CameraToOpenGLWithCallbackAndProcessingView";

        protected override bool UseOpenGL { get; set; } = true;
        protected override bool ViewRawPreview { get; set; } = false;
        protected override bool ViewProcessedPreview { get; set; } = false;
        protected override bool ProcessPreview { get; set; } = true;
        protected override bool CameraCallback { get; set; } = true;
    }
}