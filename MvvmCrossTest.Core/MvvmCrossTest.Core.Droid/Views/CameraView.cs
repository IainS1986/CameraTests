using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Support.V4;
using MvvmCrossTest.Core.Core.ViewModels;
using MvvmCrossTest.Core.Droid.Controls;
using System;
using Camera = Android.Hardware.Camera;

namespace MvvmCrossTest.Core.Droid.Views
{
    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.CameraView")]
    public class CameraView : MvxFragment<CameraViewModel>, TextureView.ISurfaceTextureListener
    {
        //TODO MOVE TO FPS CONTROL
        public DateTime LastFrameTime = DateTime.Now;

        public override string UniqueImmutableCacheTag => "CameraView";

        public Camera m_camera;
        public AutoFitTextureView m_preview;
        public ImageView m_processedPreview;
        public CameraPreviewCallback m_previewCallback;
        public TextView m_fpsDisplay;
        private byte[] m_buffer;

        private Paint m_paint;
        private TextureGLSurfaceView m_glView;

        private int m_frames;
        private DateTime m_lastFrame = DateTime.Now;

        private bool UseOpenGL { get; set; } = false;
        private bool ViewRawPreview { get; set; } = true;
        private bool ViewProcessedPreview { get; set; } = false;
        private bool ProcessPreview { get; set; } = false;
        private bool CameraCallback { get; set; } = true;

        //RGB Buffer to reduce allocation
        private int[] RGB;

        public CameraView()
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

            if(m_glView!=null)
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

        private void OnFrame(byte[] data, Camera camera)
        {
            var size = camera.GetParameters().PreviewSize;

            if (ProcessPreview)
            {
                //applyGrayScale(RGB, data, size.Width, size.Height);
                applyGrayScaleAndRotate90(RGB, data, size.Width, size.Height);
            }

            if(UseOpenGL)
            {
                Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width, Bitmap.Config.Argb8888);
                m_glView.mRenderer.LoadTexture(bm);
            }
            else if(ViewProcessedPreview)
            {
                Bitmap bm = Bitmap.CreateBitmap(RGB, size.Height, size.Width,Bitmap.Config.Argb8888);
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
            m_camera = Camera.Open();
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

                if(CameraCallback)
                {
                    m_camera.SetPreviewCallback(m_previewCallback);
                    //m_camera.SetPreviewCallbackWithBuffer(m_previewCallback);
                    //m_camera.AddCallbackBuffer(m_buffer);
                }

                m_camera.StartPreview();

                m_preview.Visibility = (ViewRawPreview || CameraCallback == false || (UseOpenGL == false && ViewProcessedPreview == false)) ? ViewStates.Visible : ViewStates.Gone;
                m_processedPreview.Visibility = (ViewProcessedPreview && CameraCallback) ? ViewStates.Visible : ViewStates.Gone;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if(m_camera != null)
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



        //TODO MOVE TO HELPER CLASS
        //https://stackoverflow.com/questions/5272388/extract-black-and-white-image-from-android-cameras-nv21-format
        /**
         * Converts YUV420 NV21 to RGB8888
         * 
         * @param data byte array on YUV420 NV21 format.
         * @param width pixels width
         * @param height pixels height
         * @return a RGB8888 pixels int array. Where each int is a pixels ARGB. 
         */
        public static int[] convertYUV420_NV21toRGB8888(byte[] data, int width, int height)
        {
            int size = width * height;
            int offset = size;
            int[] pixels = new int[size];
            int u, v, y1, y2, y3, y4;

            // i percorre os Y and the final pixels
            // k percorre os pixles U e V
            for (int i = 0, k = 0; i < size; i += 2, k += 2)
            {
                y1 = data[i] & 0xff;
                y2 = data[i + 1] & 0xff;
                y3 = data[width + i] & 0xff;
                y4 = data[width + i + 1] & 0xff;

                u = data[offset + k] & 0xff;
                v = data[offset + k + 1] & 0xff;
                u = u - 128;
                v = v - 128;

                pixels[i] = convertYUVtoRGB(y1, u, v);
                pixels[i + 1] = convertYUVtoRGB(y2, u, v);
                pixels[width + i] = convertYUVtoRGB(y3, u, v);
                pixels[width + i + 1] = convertYUVtoRGB(y4, u, v);

                if (i != 0 && (i + 2) % width == 0)
                    i += width;
            }

            return pixels;
        }

        private static int convertYUVtoRGB(int y, int u, int v)
        {
            int r, g, b;

            r = y + (int)(1.402f * v);
            g = y - (int)(0.344f * u + 0.714f * v);
            b = y + (int)(1.772f * u);
            r = r > 255 ? 255 : r < 0 ? 0 : r;
            g = g > 255 ? 255 : g < 0 ? 0 : g;
            b = b > 255 ? 255 : b < 0 ? 0 : b;
            return (int)(0xff000000 | (b << 16) | (g << 8) | r);
        }

        /**
         * Converts YUV420 NV21 to Y888 (RGB8888). The grayscale image still holds 3 bytes on the pixel.
         * 
         * @param pixels output array with the converted array o grayscale pixels
         * @param data byte array on YUV420 NV21 format.
         * @param width pixels width
         * @param height pixels height
         */
        public static void applyGrayScale(int[] pixels, byte[] data, int width, int height)
        {
            int p;
            int size = width * height;
            for (int i = 0; i < size; i++)
            {
                p = data[i] & 0xFF;
                pixels[i] = (int)(0xff000000 | p << 16 | p << 8 | p);
            }
        }

        public static void applyGrayScaleAndRotate90(int[] pixels, byte[] data, int width, int height)
        {
            int p;
            for (int y = 0, destinationColumn = height - 1; y < height; ++y, --destinationColumn)
            {
                int offset = y * width;
                for (int x = 0; x < width; x++)
                {
                    p = data[(offset + x)] & 0xFF;
                    pixels[(x * height + destinationColumn)] = (int)(0xff000000 | p << 16 | p << 8 | p);
                }
            }
        }


        public static int[] RotatePixelArrayBy90(int[] pixels, int width, int height)
        {
            int sizeBuffer = width * height;
            int[] temp = new int[sizeBuffer];

            for (int y = 0, destinationColumn = height - 1; y < height; ++y, --destinationColumn)
            {
                int offset = y * width;

                for (int x = 0; x < width; x++)
                {
                    temp[(x * height + destinationColumn)] = pixels[(offset + x)];
                }
            }

            return temp;
        }
    }
}
