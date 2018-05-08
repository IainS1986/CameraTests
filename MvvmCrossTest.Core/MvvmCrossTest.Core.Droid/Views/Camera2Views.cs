using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
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
    /// <summary>
    /// https://github.com/Fung-yuantao/android-camera2demo/blob/master/app/src/main/java/tau/camera2demo/Camera2Demo.java
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class BaseCamera2View<TViewModel> : MvxFragment<TViewModel>, TextureView.ISurfaceTextureListener where TViewModel : class, IMvxViewModel 
    {
        protected abstract int LayoutResource { get; }

        protected virtual bool Preview { get; } = true;
        protected virtual bool JNIGrayscale { get; } = false;

        //TODO MOVE TO FPS CONTROL
        public DateTime LastFrameTime = DateTime.Now;
        private int m_frames;

        public CameraDevice m_camera;
        public string m_cameraID = "0";

        public AutoFitTextureView m_preview;
        public Size m_previewSize;
        public CaptureRequest.Builder m_previewBuilder;
        public ImageReader m_imageReader;

        public CameraStateCallback m_cameraDeviceStateCallback;
        public OnImageAvailableListener m_onImageAvailableListener;
        public CameraSessionStateCallback m_cameraSessionStateCallback;

        public TextView m_fpsDisplay;

        private Handler m_handler;
        private HandlerThread m_handlerThread;

        // size of images captured in ImageReader Callback
        private int m_imageWidth = 1920; //1920
        private int m_imageHeight = 1080; //1080

        private Surface m_surface;

        public BaseCamera2View()
        {
            this.RetainInstance = true;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(LayoutResource, null);
        }

        public override void OnPause()
        {
            if (null != m_camera)
            {
                m_camera.Close();
                m_camera = null;
            }
            if (null != m_imageReader)
            {
                m_imageReader.Close();
                m_imageReader = null;
            }

            base.OnPause();
        }

        public override void OnStart()
        {
            base.OnStart();

            m_fpsDisplay = View.FindViewById<TextView>(Resource.Id.fpsLabel);
            m_fpsDisplay.SetBackgroundColor(Color.Black);
            m_fpsDisplay.SetTextColor(Color.White);

            m_preview = View.FindViewById<AutoFitTextureView>(Resource.Id.preview);
            m_preview.SurfaceTextureListener = this;

            //Init Looper
            m_handlerThread = new HandlerThread("CAMERA2");
            m_handlerThread.Start();

            m_handler = new Handler(m_handlerThread.Looper);

            //Camera2 Callback(s)
            m_cameraDeviceStateCallback = new CameraStateCallback(OnCameraDeviceOpened, OnCameraDeviceDisconnected, OnCameraDeviceError);
            m_onImageAvailableListener = new OnImageAvailableListener(OnImageAvailable);
            m_cameraSessionStateCallback = new CameraSessionStateCallback(OnSessionConfigured, OnSessionFailed);
        }

        private void OnCameraDeviceOpened(CameraDevice device)
        {
            try
            {
                m_camera = device;
                StartPreview(m_camera);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        private void OnCameraDeviceDisconnected(CameraDevice device)
        {

        }

        private void OnCameraDeviceError(CameraDevice device, CameraError error)
        {

        }

        private void OnImageAvailable(ImageReader reader)
        {
            // get the newest frame
            Image image = reader.AcquireNextImage();

            if (image == null)
            {
                return;
            }

            IntPtr jniClass = IntPtr.Zero; // JNIEnv.FindClass("MobileLib");

            //MESS WITH IMAGE HERE
            //string test = JNIUtils.AndroidInfo(JNIEnv.Handle, jniClass);

            JNIUtils.GrayscaleDisplay(JNIEnv.Handle, jniClass, image.Width, image.Height, image.GetPlanes()[0].RowStride, image.GetPlanes()[0].Buffer.Handle, m_surface.Handle);

            //Image.Plane Y_plane = image.GetPlanes()[0];
            //int Y_rowStride = Y_plane.RowStride;
            //Image.Plane U_plane = image.GetPlanes()[1];
            //int UV_rowStride = U_plane.RowStride;  //in particular, uPlane.getRowStride() == vPlane.getRowStride()
            //Image.Plane V_plane = image.GetPlanes()[2];
            //JNIUtils.RGBADisplay(JNIEnv.Handle, jniClass, image.Width, image.Height, Y_rowStride, Y_plane.Buffer.Handle, UV_rowStride, U_plane.Buffer.Handle, V_plane.Buffer.Handle, m_surface.Handle);


            image.Close();

            //UpdateFPS();
        }

        private void OnSessionConfigured(CameraCaptureSession session)
        {
            try
            {
                UpdatePreview(session);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        private void OnSessionFailed(CameraCaptureSession session)
        {

        }

        private void StartPreview(CameraDevice device)
        {
            SurfaceTexture texture = m_preview.SurfaceTexture;

            //Set Preview Size
            texture.SetDefaultBufferSize(m_previewSize.Width, m_previewSize.Height);

            m_surface = new Surface(texture);

            try
            {
                m_previewBuilder = device.CreateCaptureRequest(CameraTemplate.Preview);
            }
            catch(CameraAccessException exc)
            {
                exc.PrintStackTrace();
            }

            // to set the format of captured images and the maximum number of images that can be accessed in mImageReader
            m_imageReader = ImageReader.NewInstance(m_imageWidth, m_imageHeight, ImageFormatType.Yuv420888, 2);
            m_imageReader.SetOnImageAvailableListener(m_onImageAvailableListener, m_handler);

            // the first added target surface is for camera PREVIEW display
            // the second added target mImageReader.getSurface() is for ImageReader Callback where we can access EACH frame
            if(Preview)
                m_previewBuilder.AddTarget(m_surface);
            else if(JNIGrayscale)
                m_previewBuilder.AddTarget(m_imageReader.Surface);

            //output Surface
            List<Surface> outputSurfaces = new List<Surface>();
            if (Preview)
                outputSurfaces.Add(m_surface);
            else if (JNIGrayscale)
                outputSurfaces.Add(m_imageReader.Surface);

            /*camera.createCaptureSession(
                    Arrays.asList(surface, mImageReader.getSurface()),
                    mSessionStateCallback, mHandler);
                    */
            device.CreateCaptureSession(outputSurfaces, m_cameraSessionStateCallback, m_handler);
        }

        private void UpdatePreview(CameraCaptureSession session)
        {
            m_previewBuilder.Set(CaptureRequest.ControlAfMode, new Java.Lang.Integer((int)ControlAFMode.Auto));

            session.SetRepeatingRequest(m_previewBuilder.Build(), null, m_handler);
        }

        private void UpdateFPS()
        {
            DateTime now = DateTime.Now;    
            if (now - LastFrameTime > TimeSpan.FromMilliseconds(1000))
            {
                string format = "{0}";

                Activity.RunOnUiThread(() =>
                {
                    m_fpsDisplay.Text = string.Format(format, m_frames.ToString());
                });

                LastFrameTime = now;
                m_frames = 0;
            }
            m_frames++;
        }

        #region TextureView.ISurfaceTextureListener

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            try
            {
                // to get the manager of all cameras
                CameraManager cameraManager = (CameraManager)Activity.GetSystemService(Context.CameraService);
                // to get features of the selected camera
                CameraCharacteristics characteristics = cameraManager.GetCameraCharacteristics(m_cameraID);
                // to get stream configuration from features
                StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                // to get the size that the camera supports
                m_previewSize = map.GetOutputSizes((int)ImageFormatType.Jpeg)[0];

                // open camera
                //if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != PackageManager.PermissionGranted)
                //{
                //    // TODO: Consider calling
                //    //    ActivityCompat#requestPermissions
                //    // here to request the missing permissions, and then overriding
                //    //   public void onRequestPermissionsResult(int requestCode, String[] permissions,
                //    //                                          int[] grantResults)
                //    // to handle the case where the user grants the permission. See the documentation
                //    // for ActivityCompat#requestPermissions for more details.
                //    requestCameraPermission();
                //    return;
                //}
                cameraManager.OpenCamera(m_cameraID, m_cameraDeviceStateCallback, m_handler);
            }
            catch(CameraAccessException exp)
            {
                exp.PrintStackTrace();
            }
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return false;
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
    [Register("mvvmcrosstest.core.droid.views.Camera2PreviewView")]
    public class Camera2PreviewView : BaseCamera2View<Camera2PreviewViewModel>
    {
        public override string UniqueImmutableCacheTag => "Camera2PreviewView";

        protected override bool Preview => true;
        protected override bool JNIGrayscale => false;

        protected override int LayoutResource => Resource.Layout.Camera2PreviewView;

    }

    [MvxFragment(typeof(DrawerViewModel), Resource.Id.frameLayout)]
    [Register("mvvmcrosstest.core.droid.views.Camera2JNIGrayscaleView")]
    public class Camera2JNIGrayscaleView : BaseCamera2View<Camera2JNIGrayscaleViewModel>
    {
        public override string UniqueImmutableCacheTag => "Camera2JNIGrayscaleView";

        protected override bool Preview => false;
        protected override bool JNIGrayscale => true;

        protected override int LayoutResource => Resource.Layout.Camera2JNIGrayscaleView;

    }
}