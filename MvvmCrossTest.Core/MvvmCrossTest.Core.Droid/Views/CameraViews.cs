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
using ApxLabs.FastAndroidCamera;
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

        //Fast Android Camera
        public FastCameraPreviewCallback m_fastPreviewCallback;

        private Paint m_paint;
        private TextureGLSurfaceView m_glView;

        private int m_frames;
        private DateTime m_lastFrame = DateTime.Now;

        protected virtual bool UseOpenGL { get; set; } = false;
        protected virtual bool ViewRawPreview { get; set; } = true;
        protected virtual bool ViewProcessedPreview { get; set; } = false;
        protected virtual bool ProcessPreview { get; set; } = false;
        protected virtual bool CameraCallback { get; set; } = true;
        protected virtual bool UseFastAndroidCamera { get; set; } = false;

        //RGB Buffer to reduce allocation
        private int[] RGB;

        //Bitmap re-used for rendering
        private Bitmap m_bitmap;
        private bool FirstFrame { get; set; } = true;

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

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            if(m_bitmap!=null)
            {
                m_bitmap.Dispose();
                m_bitmap = null;
            }

            if(m_paint!=null)
            {
                m_paint.Dispose();
                m_paint = null;
            }

            if(m_glView != null)
            {
                m_glView.Dispose();
                m_glView = null;
            }
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

            UpdateBitmapPreviews(size);

            //m_camera.AddCallbackBuffer(m_buffer);

            UpdateFPS();
        }

        private void OnFastFrame(IntPtr data, Android.Hardware.Camera camera)
        {
            var size = camera.GetParameters().PreviewSize;

            // Wrap the JNI reference to the Java byte array
            using (FastJavaByteArray buffer = new FastJavaByteArray(data))
            {
                // Pass it to native APIs
                if(ProcessPreview)
                {
                    GraphicsHelper.applyGrayScaleAndRotate90(RGB, buffer, size.Width, size.Height);
                }

                UpdateBitmapPreviews(size);

                // reuse the Java byte array; return it to the Camera API
                camera.AddCallbackBuffer(buffer);

                // Don't do anything else with the buffer at this point - it now "belongs" to
                // Android, and the Camera could overwrite the data at any time.
            }
            // The end of the using() block calls Dispose() on the buffer, releasing our JNI
            // reference to the array

            UpdateFPS();
        }

        private void UpdateBitmapPreviews(Android.Hardware.Camera.Size size)
        {
            if (UseOpenGL)
            {
                //Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width, Bitmap.Config.Argb8888);
                m_bitmap.SetPixels(RGB, 0, size.Height, 0, 0, size.Height, size.Width);

                if (FirstFrame)
                {
                    m_glView.mRenderer.LoadTexture(m_bitmap);
                }
                else
                {
                    m_glView.mRenderer.UpdateTexture();
                }
            }
            else if (ViewProcessedPreview)
            {
                //Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width, Bitmap.Config.Argb8888);
                m_bitmap.SetPixels(RGB, 0, size.Height, 0, 0, size.Height, size.Width);

                //Strangely, it seems faster doing this every frame than just first frame and updating texture. Maybe threading issue?
                m_processedPreview.SetImageBitmap(m_bitmap);
            }
        }

        private void UpdateFPS()
        {
            FirstFrame = false;

            DateTime now = DateTime.Now;
            if (now - LastFrameTime > TimeSpan.FromMilliseconds(1000))
            {
                string format = (UseOpenGL) ? "{0} ({1})" : "{0}";

                m_fpsDisplay.Text = string.Format(format, m_frames.ToString(), m_glView?.mRenderer?.FPS.ToString());

                LastFrameTime = now;
                m_frames = 0;
            }
            m_frames++;
        }

        #region TextureView.ISurfaceTextureListener

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            m_camera = Android.Hardware.Camera.Open();

            //Generate single Bitmap for preview
            var size = m_camera.GetParameters().PreviewSize;
            m_bitmap = Bitmap.CreateBitmap(size.Height, size.Width, Bitmap.Config.Argb8888);

            m_previewCallback = new CameraPreviewCallback(OnFrame);
            m_fastPreviewCallback = new FastCameraPreviewCallback(OnFastFrame);

            var format = m_camera.GetParameters().PreviewFormat;
            int totalpixels = size.Width * size.Height;
            RGB = new int[totalpixels];

            int totalBytes = (totalpixels * ImageFormat.GetBitsPerPixel(format)) / 8;

            //Hacking just for my moto g4 https://stackoverflow.com/questions/3841122/android-camera-preview-is-sideways
            m_camera.SetDisplayOrientation(90);

            try
            {
                m_camera.SetPreviewTexture(surface);

                if (CameraCallback)
                {
                    if(UseFastAndroidCamera)
                    {
                        int NUM_PREVIEW_BUFFERS = 1;
                        for (uint i = 0; i < NUM_PREVIEW_BUFFERS; ++i)
                        {
                            using (FastJavaByteArray buffer = new FastJavaByteArray(totalBytes))
                            {
                                // allocate new Java byte arrays for Android to use for preview frames
                                m_camera.AddCallbackBuffer(new FastJavaByteArray(totalBytes));
                            }
                            // The using block automatically calls Dispose() on the buffer, which is safe
                            // because it does not automaticaly destroy the Java byte array. It only releases
                            // our JNI reference to that array; the Android Camera (in Java land) still
                            // has its own reference to the array.
                        }

                        // non-marshaling version of the preview callback
                        m_camera.SetNonMarshalingPreviewCallback(m_fastPreviewCallback);
                    }
                    else
                    {
                        m_camera.SetPreviewCallback(m_previewCallback);
                        //m_camera.SetPreviewCallbackWithBuffer(m_previewCallback);

                        //m_buffer = new byte[totalBytes];
                        //m_camera.AddCallbackBuffer(m_buffer);
                    }
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