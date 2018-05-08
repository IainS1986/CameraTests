using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Text;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using MvvmCrossTest.Core.Droid.Controls;
using MvvmCrossTest.Core.Droid.Helper;

namespace MvvmCrossTest.Core.Droid.Services
{
    public enum CameraStateEnum
    {
        STATE_CLOSED = 0,
        STATE_OPENED = 1,
        STATE_PREVIEW = 2,
        STATE_WAITING_FOR_3A_CONVERGENCE = 3
    }

    /// <summary>
    /// https://github.com/xamarin/monodroid-samples/blob/master/android5.0/Camera2Raw/Camera2RawFragment.cs
    /// </summary>
    public class DroidCameraService
    {
        // Timeout for the pre-capture sequence.
        private const long PRECAPTURE_TIMEOUT_MS = 1000;

        // Tolerance when comparing aspect ratios.
        private const double ASPECT_RATIO_TOLERANCE = 0.005;

        // Conversion from screen rotation to JPEG orientation.
        private readonly SparseIntArray ORIENTATIONS = new SparseIntArray();

        private Activity m_currentActivity;
        public Activity CurrentActivity
        {
            get { return m_currentActivity; }
            set { m_currentActivity = value; }
        }

        private Android.Support.V4.App.FragmentManager m_fragmentManager;
        public Android.Support.V4.App.FragmentManager CurrentFragmentManager
        {
            get { return m_fragmentManager; }
            set { m_fragmentManager = value; }
        }

        /// <summary>
        /// A {@link Handler} for running tasks in the background.
        /// </summary>
        private Handler m_backgroundHandler;
        public Handler BackgroundHandler
        {
            get { return m_backgroundHandler; }
            set { m_backgroundHandler = value; }
        }

        /// <summary>
        /// An additional thread for running tasks that shouldn't block the UI.  This is used for all
        /// callbacks from the {@link CameraDevice} and {@link CameraCaptureSession}s.
        /// </summary>
        private HandlerThread m_backgroundThread;
        public HandlerThread BackgroundThread
        {
            get { return m_backgroundThread; }
            set { m_backgroundThread = value; }
        }

        private CameraStateEnum m_state = CameraStateEnum.STATE_CLOSED;
        public CameraStateEnum State
        {
            get { return m_state; }
            set { m_state = value; }
        }

        /// <summary>
        /// A lock protecting camera state.
        /// </summary>
        readonly object m_cameraStateLock = new object();
        public object CameraStateLock
        {
            get { return m_cameraStateLock; }
        }

        /// <summary>
        /// A {@link Semaphore} to prevent the app from exiting before closing the camera.
        /// </summary>
        readonly Semaphore m_cameraOpenCloseLock = new Semaphore(1);
        public Semaphore CameraOpenCloseLock
        {
            get { return m_cameraOpenCloseLock; }
        }

        /// <summary>
        /// {@link CaptureRequest.Builder} for the camera preview
        /// </summary>
        private CaptureRequest.Builder m_previewRequestBuilder;
        public CaptureRequest.Builder PreviewRequestBuilder
        {
            get { return m_previewRequestBuilder; }
            set { m_previewRequestBuilder = value; }
        }

        /// <summary>
        /// A reference counted holder wrapping the {@link ImageReader} that handles JPEG image captures.
        /// This is used to allow us to clean up the {@link ImageReader} when all background tasks using
        /// its {@link Image}s have completed.
        /// </summary>
        private RefCountedAutoCloseable<ImageReader> m_jpegImageReader;
        public RefCountedAutoCloseable<ImageReader> JpegImageReader
        {
            get { return m_jpegImageReader; }
            set { m_jpegImageReader = value; }
        }

        /// <summary>
        /// A reference counted holder wrapping the {@link ImageReader} that handles RAW image captures.
        /// This is used to allow us to clean up the {@link ImageReader} when all background tasks using
        /// its {@link Image}s have completed.
        /// </summary>
        private RefCountedAutoCloseable<ImageReader> m_rawImageReader;
        public RefCountedAutoCloseable<ImageReader> RawImageReader
        {
            get { return m_rawImageReader; }
            set { m_rawImageReader = value; }
        }

        /// <summary>
        /// Request ID to {@link ImageSaver.ImageSaverBuilder} mapping for in-progress JPEG captures.
        /// </summary>
        private readonly TreeMap m_jpegResultQueue = new TreeMap();
        public TreeMap JpegResultQueue
        {
            get { return m_jpegResultQueue; }
        }

        /// <summary>
        /// Request ID to {@link ImageSaver.ImageSaverBuilder} mapping for in-progress RAW captures.
        /// </summary>
        private readonly TreeMap m_rawResultQueue = new TreeMap();
        public TreeMap RawResultQueue
        {
            get { return m_rawResultQueue; }
        }

        /// <summary>
        /// A {@link CameraCaptureSession } for camera preview.
        /// </summary>
        private CameraCaptureSession m_captureSession;
        public CameraCaptureSession CaptureSession
        {
            get { return m_captureSession; }
            set { m_captureSession = value; }
        }

        private CameraDevice m_cameraDevice;
        public CameraDevice Device
        {
            get { return m_cameraDevice; }
            set { m_cameraDevice = value; }
        }

        /// <summary>
        /// The {@link CameraCharacteristics} for the currently configured camera device.
        /// </summary>
        private CameraCharacteristics m_characteristics;
        public CameraCharacteristics Characteristics
        {
            get { return m_characteristics; }
            set { m_characteristics = value; }
        }

        /// <summary>
        /// Whether or not the currently configured camera device is fixed-focus.
        /// </summary>
        private bool m_noAFRun = false;
        public bool NoAFRun
        {
            get { return m_noAFRun; }
            set { m_noAFRun = value; }
        }

        private Size m_previewSize;
        public Size PreviewSize
        {
            get { return m_previewSize; }
            set { m_previewSize = value; }
        }

        private AutoFitTextureView m_textureView;
        public AutoFitTextureView TextureView
        {
            get { return m_textureView; }
            set { m_textureView = value; }
        }

        /// <summary>>
        /// A {@link CameraCaptureSession.CaptureCallback} that handles events for the preview and
        /// pre-capture sequence.
        /// </summary>
        private CameraCaptureSession.CaptureCallback m_preCaptureCallback;
        public CameraCaptureSession.CaptureCallback PreCaptureCallback
        {
            get { return m_preCaptureCallback; }
            set { m_preCaptureCallback = value; }
        }

        /// <summary>
        /// A {@link CameraCaptureSession.CaptureCallback} that handles the still JPEG and RAW capture
        /// request.
        /// </summary>
        private CameraCaptureSession.CaptureCallback m_captureCallback;
        public CameraCaptureSession.CaptureCallback CaptureCallback
        {
            get { return m_captureCallback; }
            set { m_captureCallback = value; }
        }

        /// <summary>
        /// {@link CameraDevice.StateCallback} is called when the currently active {@link CameraDevice}
        /// changes its state.
        /// </summary>
        private CameraDevice.StateCallback m_stateCallback;
        public CameraDevice.StateCallback StateCallback
        {
            get { return m_stateCallback; }
            set { m_stateCallback = value; }
        }

        /// <summary>
        /// Timer to use with pre-capture sequence to ensure a timely capture if 3A convergence is taking
        /// too long.
        /// </summary>
        private long m_captureTimer;
        public long CaptureTimer
        {
            get { return m_captureTimer; }
            set { m_captureTimer = value; }
        }

        /// <summary>
        /// Number of pending user requests to capture a photo.
        /// </summary>
        private int m_pendingUserCaptures = 0;
        public int PendingUserCaptures
        {
            get { return m_pendingUserCaptures; }
            set { m_pendingUserCaptures = value; }
        }

        /// <summary>
        /// A counter for tracking corresponding {@link CaptureRequest}s and {@link CaptureResult}s
        /// across the {@link CameraCaptureSession} capture callbacks.
        /// </summary>
        private readonly AtomicInteger m_requestCounter = new AtomicInteger();
        public AtomicInteger RequestCounter
        {
            get { return m_requestCounter; }
        }

        /// <summary>
        /// An {@link OrientationEventListener} used to determine when device rotation has occurred.
        /// This is mainly necessary for when the device is rotated by 180 degrees, in which case
        ///	onCreate or onConfigurationChanged is not called as the view dimensions remain the same,
        ///	but the orientation of the has changed, and thus the preview rotation must be updated..
        /// </summary>
        private OrientationEventListener m_orientationListener;
        public OrientationEventListener OrientationListener
        {
            get { return m_orientationListener; }
            set { m_orientationListener = value; }
        }

        /// <summary>
        /// A {@link Handler} for showing {@link Toast}s on the UI thread.
        /// </summary>
        private Handler m_messageHandler;
        public Handler MessageHandler
        {
            get { return m_messageHandler; }
            set {  m_messageHandler = value; }
        }

        /// <summary>
        /// {@link TextureView.SurfaceTextureListener} handles several lifecycle events of a
        /// {@link TextureView}.
        /// </summary>
        private TextureView.ISurfaceTextureListener m_surfaceTextureListener;
        public TextureView.ISurfaceTextureListener SurfaceTextureListener
        {
            get { return m_surfaceTextureListener; }
            set { m_surfaceTextureListener = value; }
        }

        /// <summary>
        /// ID of the current {@link CameraDevice}.
        /// </summary>
        private string m_cameraId;
        public string CameraId
        {
            get { return m_cameraId; }
            set { m_cameraId = value; }
        }

        /// <summary>
        /// This a callback object for the {@link ImageReader}. "onImageAvailable" will be called when a
        /// JPEG image is ready to be saved.
        /// </summary>
        private ImageReader.IOnImageAvailableListener m_onJpegImageAvailableListener;
        public ImageReader.IOnImageAvailableListener OnJpegImageAvailableListener
        {
            get { return m_onJpegImageAvailableListener; }
            set { m_onJpegImageAvailableListener = value; }
        }

        /// <summary>
        /// This a callback object for the {@link ImageReader}. "onImageAvailable" will be called when a
        /// RAW image is ready to be saved.
        /// </summary>
        private ImageReader.IOnImageAvailableListener m_onRawImageAvailableListener;
        public ImageReader.IOnImageAvailableListener OnRawImageAvailableListener
        {
            get { return m_onRawImageAvailableListener; }
            set { m_onRawImageAvailableListener = value; }
        }

        public DroidCameraService(Activity activity, Android.Support.V4.App.FragmentManager manager)
        {
            CurrentActivity = activity;
            CurrentFragmentManager = manager;

            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 180);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 270);

            CaptureCallback = new CameraCaptureCallback(OnCameraCaptureStarted,
                                                        OnCameraCaptureFinished,
                                                        OnCameraCaptureFailed);

            OnJpegImageAvailableListener = new OnImageAvailableListener(OnJpegImageAvailable);
            OnRawImageAvailableListener = new OnImageAvailableListener(OnRawImageAvailable);
        }

        #region Fragment Functions
        public void OnViewCreated(Activity activity,
                                   Android.Support.V4.App.FragmentManager manager,
                                  AutoFitTextureView textureView)
        {
            CurrentActivity = activity;
            CurrentFragmentManager = manager;
            TextureView = textureView;

            // Setup a new OrientationEventListener.  This is used to handle rotation events like a
            // 180 degree rotation that do not normally trigger a call to onCreate to do view re-layout
            // or otherwise cause the preview TextureView's size to change.
            OrientationListener = new DeviceOrientationListener(CurrentActivity, SensorDelay.Normal, OnOrientationChanged);
            MessageHandler = new MessageHandler(Looper.MainLooper, OnMessage);
            PreCaptureCallback = new PreCameraCaptureCallback(OnPreCameraCaptureProcess);
            SurfaceTextureListener = new SurfaceTextureListener(OnSurfaceTextureAvailable, OnSurfaceTextureDestroyed, OnSurfaceTextureSizeChanged);
            StateCallback = new CameraStateCallback(OnCameraOpened, OnCameraDisconnected, OnCameraError);
        }

        public void OnResume()
        {
            StartBackgroundThread();

            if (CanOpenCamera())
            {

                // When the screen is turned off and turned back on, the SurfaceTexture is already
                // available, and "onSurfaceTextureAvailable" will not be called. In that case, we should
                // configure the preview bounds here (otherwise, we wait until the surface is ready in
                // the SurfaceTextureListener).
                if (TextureView.IsAvailable)
                {
                    ConfigureTransform(TextureView.Width, TextureView.Height, CurrentActivity);
                }
                else
                {
                    TextureView.SurfaceTextureListener = SurfaceTextureListener;
                }
                if (OrientationListener != null && OrientationListener.CanDetectOrientation())
                {
                    OrientationListener.Enable();
                }
            }
        }

        public void OnPause()
        {
            if (OrientationListener != null)
            {
                OrientationListener.Disable();
            }
            CloseCamera();
            StopBackgroundThread();
        }
        #endregion

        #region SurfaceTextureListener Callbacks
        /// <summary>
        /// Callback from SurfaceTextureListener implementor
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            ConfigureTransform(width, height, CurrentActivity);
        }

        /// <summary>
        /// Callback from SurfaceTextureListener implementor
        /// </summary>
        /// <param name="surface"></param>
        public void OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            lock (CameraStateLock)
            {
                PreviewSize = null;
            }
        }

        /// <summary>
        /// Callback from SurfaceTextureListener implementor
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            ConfigureTransform(width, height, CurrentActivity);
        }

        #endregion

        #region MessageHandler Callbacks
        /// <summary>
        /// Callback from MessageHandler implementor
        /// </summary>
        /// <param name="msg"></param>
        public void OnMessage(Message msg)
        {
            OnShowToast((string)msg.Obj);
        }
        #endregion

        #region OrientationListener Callbacks
        /// <summary>
        /// Callback from OrientationListener implementor
        /// </summary>
        /// <param name="orientation"></param>
        public void OnOrientationChanged(int orientation)
        {
            if (TextureView != null && TextureView.IsAvailable)
            {
                ConfigureTransform(TextureView.Width, TextureView.Height, CurrentActivity);
            }
        }
        #endregion

        #region CameraStateCallbacks 
        /// <summary>
        /// Callback from the CameraStateCallback implementor
        /// </summary>
        /// <param name="device"></param>
        public void OnCameraOpened(CameraDevice device)
        {
            lock (CameraStateLock)
            {
                State = CameraStateEnum.STATE_OPENED;
                CameraOpenCloseLock.Release();
                Device = device;

                // Start the preview session if the TextureView has been set up already.
                if (PreviewSize != null && TextureView.IsAvailable)
                {
                    CreateCameraPreviewSessionLocked();
                }
            }
        }

        /// <summary>
        /// Callback from the CameraStateCallback implementor
        /// </summary>
        /// <param name="device"></param>
        public void OnCameraDisconnected(CameraDevice device)
        {
            lock (CameraStateLock)
            {
                State = CameraStateEnum.STATE_CLOSED;
                CameraOpenCloseLock.Release();
                device.Close();
                Device = null;
            }
        }

        /// <summary>
        /// Callback from the CameraStateCallback implementor
        /// </summary>
        /// <param name="device"></param>
        /// <param name="error"></param>
        public void OnCameraError(CameraDevice device, Android.Hardware.Camera2.CameraError error)
        {
            Log.Error("DroidCameraService.OnCameraError", "Received camera device error: " + error);
            lock (CameraStateLock)
            {
                State = CameraStateEnum.STATE_CLOSED;
                CameraOpenCloseLock.Release();
                device.Close();
                Device = null;
            }

            var activity = CurrentActivity;
            if (null != activity)
            {
                activity.Finish();
            }
        }
        #endregion

        #region CameraPreviewCaptureCallbacks
        /// <summary>
        /// Callback from the CameraPreviewCapture implementor
        /// </summary>
        public void OnCameraPreviewCaptureConfigured(CameraCaptureSession cameraCaptureSession)
        {
            lock (CameraStateLock)
            {
                // The camera is already closed
                if (null == Device)
                    return;

                try
                {
                    Setup3AControlsLocked(PreviewRequestBuilder);

                    // Finally, we start displaying the camera preview.
                    cameraCaptureSession.SetRepeatingRequest(
                        PreviewRequestBuilder.Build(),
                        PreCaptureCallback, 
                        BackgroundHandler);

                    State = CameraStateEnum.STATE_PREVIEW;
                }
                catch (CameraAccessException e)
                {
                    e.PrintStackTrace();
                    return;
                }
                catch (IllegalStateException e)
                {
                    e.PrintStackTrace();
                    return;
                }
                // When the session is ready, we start displaying the preview.
                CaptureSession = cameraCaptureSession;
            }
        }
        #endregion

        #region PreCameraCaptureCallbacks
        /// <summary>
        /// Callback from the PreCameraCaputreCallback implementor
        /// </summary>
        public void OnPreCameraCaptureProcess(CaptureResult result)
        {
            lock (CameraStateLock)
            {
                switch (State)
                {
                    case CameraStateEnum.STATE_PREVIEW:
                        // We have nothing to do when the camera preview is running normally.
                        break;
                    case CameraStateEnum.STATE_WAITING_FOR_3A_CONVERGENCE:
                        bool readyToCapture = true;
                        if (!NoAFRun)
                        {
                            var afState = (Integer)result.Get(CaptureResult.ControlAfState);
                            if (afState == null)
                            {
                                break;
                            }
                            // If auto-focus has reached locked state, we are ready to capture
                            readyToCapture = (afState.IntValue() == (int)ControlAFState.FocusedLocked ||
                            afState.IntValue() == (int)ControlAFState.NotFocusedLocked);
                        }

                        // If we are running on an non-legacy device, we should also wait until
                        // auto-exposure and auto-white-balance have converged as well before
                        // taking a picture.
                        if (!IsLegacyLocked())
                        {
                            var aeState = (Integer)result.Get(CaptureResult.ControlAeState);
                            var awbState = (Integer)result.Get(CaptureResult.ControlAwbState);
                            if (aeState == null || awbState == null)
                            {
                                break;
                            }

                            readyToCapture = readyToCapture &&
                            aeState.IntValue() == (int)ControlAEState.Converged &&
                            awbState.IntValue() == (int)ControlAwbState.Converged;
                        }

                        // If we haven't finished the pre-capture sequence but have hit our maximum
                        // wait timeout, too bad! Begin capture anyway.
                        if (!readyToCapture && HitTimeoutLocked())
                        {
                            Log.Warn("DroidCameraService", "Timed out waiting for pre-capture sequence to complete.");
                            readyToCapture = true;
                        }

                        if (readyToCapture && PendingUserCaptures > 0)
                        {
                            // Capture once for each user tap of the "Picture" button.
                            while (PendingUserCaptures > 0)
                            {
                                CaptureStillPictureLocked(CurrentActivity);
                                PendingUserCaptures--;
                            }
                            // After this, the camera will go back to the normal state of preview.
                            State = CameraStateEnum.STATE_PREVIEW;
                        }
                        break;
                }
            }
        }
        #endregion

        #region CameraCaptureCallbacks
        /// <summary>
        /// Callback from CameraCaptureCallback
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="timestamp"></param>
        /// <param name="frameNumber"></param>
        public void OnCameraCaptureStarted(CameraCaptureSession session,
                                                CaptureRequest request,
                                                long timestamp,
                                                long frameNumber)
        {
            string currentDateTime = GenerateTimestamp();
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var rawFilePath = System.IO.Path.Combine(path, "RAW_" + currentDateTime + ".dng");
            var jpegFilePath = System.IO.Path.Combine(path, "JPEG_" + currentDateTime + ".jpg");
            var rawFile = new FileInfo(rawFilePath);
            var jpegFile = new FileInfo(jpegFilePath);

            // Look up the ImageSaverBuilder for this request and update it with the file name
            // based on the capture start time.
            ImageSaverBuilder jpegBuilder;
            ImageSaverBuilder rawBuilder;
            int requestId = (int)request.Tag;
            lock (CameraStateLock)
            {
                jpegBuilder = (ImageSaverBuilder)JpegResultQueue.Get(requestId);
                rawBuilder = (ImageSaverBuilder)RawResultQueue.Get(requestId);
            }

            if (jpegBuilder != null)
                jpegBuilder.SetFile(jpegFile);

            if (rawBuilder != null)
                rawBuilder.SetFile(rawFile);
        }

        /// <summary>
        /// Callback from CameraCaptureCallback
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void OnCameraCaptureFinished(CameraCaptureSession session,
                                                CaptureRequest request,
                                                TotalCaptureResult result)
        {
            int requestId = (int)request.Tag;
            ImageSaverBuilder jpegBuilder;
            ImageSaverBuilder rawBuilder;
            var sb = new System.Text.StringBuilder();

            // Look up the ImageSaverBuilder for this request and update it with the CaptureResult
            lock (CameraStateLock)
            {
                jpegBuilder = (ImageSaverBuilder)JpegResultQueue.Get(requestId);
                rawBuilder = (ImageSaverBuilder)RawResultQueue.Get(requestId);

                // If we have all the results necessary, save the image to a file in the background.
                HandleCompletionLocked(requestId, jpegBuilder, JpegResultQueue);
                HandleCompletionLocked(requestId, rawBuilder, RawResultQueue);

                if (jpegBuilder != null)
                {
                    jpegBuilder.SetResult(result);
                    sb.Append("Saving JPEG as: ");
                    sb.Append(jpegBuilder.GetSaveLocation());
                }
                if (rawBuilder != null)
                {
                    rawBuilder.SetResult(result);
                    if (jpegBuilder != null)
                        sb.Append(", ");
                    sb.Append("Saving RAW as: ");
                    sb.Append(rawBuilder.GetSaveLocation());
                }
                FinishedCaptureLocked();
            }

            OnShowToast(sb.ToString());
        }

        /// <summary>
        /// Callback from CameraCaptureCallback
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="failure"></param>
        public void OnCameraCaptureFailed(CameraCaptureSession session, 
                                            CaptureRequest request,
                                            CaptureFailure failure)
        {
            int requestId = (int)request.Tag;
            lock (CameraStateLock)
            {
                JpegResultQueue.Remove(requestId);
                RawResultQueue.Remove(requestId);
                FinishedCaptureLocked();
            }

            OnShowToast("Capture failed!");
        }
        #endregion

        #region ImageAvailable Callbacks
        public void OnJpegImageAvailable(ImageReader reader)
        {
            DequeueAndSaveImage(JpegResultQueue, JpegImageReader);
        }

        public void OnRawImageAvailable(ImageReader reader)
        {
            DequeueAndSaveImage(RawResultQueue, RawImageReader);
        }
        #endregion

        #region ToastCallbacks
        /// <summary>
        /// Callback to show a toast on screen
        /// </summary>
        /// <param name="message"></param>
        public void OnShowToast(string message)
        {
            DroidToastService.ShowToast("Failed to configure camera.", CurrentActivity);
        }
        #endregion

        #region Internal Private Functions

        /// <summary>
        /// Retrieve the next {@link Image} from a reference counted {@link ImageReader}, retaining
        /// that {@link ImageReader} until that {@link Image} is no longer in use, and set this
        /// {@link Image} as the result for the next request in the queue of pending requests.  If
        /// all necessary information is available, begin saving the image to a file in a background
        /// thread.
        /// </summary>
        /// <param name="pendingQueue">the currently active requests.</param>
        /// <param name="reader">a reference counted wrapper containing an {@link ImageReader} from which to acquire an image.</param>
        private void DequeueAndSaveImage(TreeMap pendingQueue,
                                         RefCountedAutoCloseable<ImageReader> reader)
        {
            lock (CameraStateLock)
            {
                IMapEntry entry = pendingQueue.FirstEntry();
                var builder = (ImageSaverBuilder)entry.Value;

                // Increment reference count to prevent ImageReader from being closed while we
                // are saving its Images in a background thread (otherwise their resources may
                // be freed while we are writing to a file).
                if (reader == null || reader.GetAndRetain() == null)
                {
                    Log.Error("DroidCameraService", "Paused the activity before we could save the image," +
                    " ImageReader already closed.");
                    pendingQueue.Remove(entry.Key);
                    return;
                }

                Image image;
                try
                {
                    image = reader.Get().AcquireNextImage();
                }
                catch (IllegalStateException)
                {
                    Log.Error("DroidCameraService", "Too many images queued for saving, dropping image for request: " +
                    entry.Key);
                    pendingQueue.Remove(entry.Key);
                    return;
                }

                builder.SetRefCountedReader(reader).SetImage(image);

                HandleCompletionLocked((int)entry.Key, builder, pendingQueue);
            }
        }

        /// <summary>
        /// Configure the necessary {@link android.graphics.Matrix} transformation to `mTextureView`,
        /// and start/restart the preview capture session if necessary.
        ///
        /// This method should be called after the camera state has been initialized in
        /// setUpCameraOutputs.
        /// </summary>
        /// <param name="viewWidth">The width of `mTextureView`</param>
        /// <param name="viewHeight">The height of `mTextureView`</param>
        private void ConfigureTransform(int viewWidth, int viewHeight, Activity activity)
        {
            lock (CameraStateLock)
            {
                if (TextureView == null || activity == null)
                {
                    return;
                }

                var map = (StreamConfigurationMap) Characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

                // For still image captures, we always use the largest available size.
                Size largestJpeg = (Size)Collections.Max(Arrays.AsList(map.GetOutputSizes((int)ImageFormatType.Jpeg)),
                                       new CompareSizesByArea());

                // Find the rotation of the device relative to the native device orientation.
                var deviceRotation = (int)activity.WindowManager.DefaultDisplay.Rotation;

                // Find the rotation of the device relative to the camera sensor's orientation.
                int totalRotation = SensorToDeviceRotation(Characteristics, deviceRotation);

                // Swap the view dimensions for calculation as needed if they are rotated relative to
                // the sensor.
                bool swappedDimensions = totalRotation == 90 || totalRotation == 270;
                int rotatedViewWidth = viewWidth;
                int rotatedViewHeight = viewHeight;
                if (swappedDimensions)
                {
                    rotatedViewWidth = viewHeight;
                    rotatedViewHeight = viewWidth;
                }

                // Find the best preview size for these view dimensions and configured JPEG size.
                Size previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                                       rotatedViewWidth, rotatedViewHeight, largestJpeg);

                if (swappedDimensions)
                {
                    TextureView.SetAspectRatio(
                        previewSize.Height, previewSize.Width);
                }
                else
                {
                    TextureView.SetAspectRatio(
                        previewSize.Width, previewSize.Height);
                }

                // Find rotation of device in degrees (reverse device orientation for front-facing
                // cameras).
                int rotation = ((int)Characteristics.Get(CameraCharacteristics.LensFacing) ==
                               (int)LensFacing.Front) ?
                    (360 + ORIENTATIONS.Get(deviceRotation)) % 360 :
                    (360 - ORIENTATIONS.Get(deviceRotation)) % 360;

                Matrix matrix = new Matrix();
                RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
                RectF bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
                float centerX = viewRect.CenterX();
                float centerY = viewRect.CenterY();

                // Initially, output stream images from the Camera2 API will be rotated to the native
                // device orientation from the sensor's orientation, and the TextureView will default to
                // scaling these buffers to fill it's view bounds.  If the aspect ratios and relative
                // orientations are correct, this is fine.
                //
                // However, if the device orientation has been rotated relative to its native
                // orientation so that the TextureView's dimensions are swapped relative to the
                // native device orientation, we must do the following to ensure the output stream
                // images are not incorrectly scaled by the TextureView:
                //   - Undo the scale-to-fill from the output buffer's dimensions (i.e. its dimensions
                //     in the native device orientation) to the TextureView's dimension.
                //   - Apply a scale-to-fill from the output buffer's rotated dimensions
                //     (i.e. its dimensions in the current device orientation) to the TextureView's
                //     dimensions.
                //   - Apply the rotation from the native device orientation to the current device
                //     rotation.
                if (deviceRotation == (int)SurfaceOrientation.Rotation90 || deviceRotation == (int)SurfaceOrientation.Rotation270)
                {
                    bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                    matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                    float scale = System.Math.Max(
                                      (float)viewHeight / previewSize.Height,
                                      (float)viewWidth / previewSize.Width);
                    matrix.PostScale(scale, scale, centerX, centerY);

                }
                matrix.PostRotate(rotation, centerX, centerY);

                TextureView.SetTransform(matrix);

                // Start or restart the active capture session if the preview was initialized or
                // if its aspect ratio changed significantly.
                if (PreviewSize == null || !CheckAspectsEqual(previewSize, PreviewSize))
                {
                    PreviewSize = previewSize;
                    if (State != CameraStateEnum.STATE_CLOSED)
                    {
                        CreateCameraPreviewSessionLocked();
                    }
                }
            }
        }


        /// <summary>
        /// Creates a new {@link CameraCaptureSession} for camera preview.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        private void CreateCameraPreviewSessionLocked()
        {
            try
            {
                SurfaceTexture texture = TextureView.SurfaceTexture;
                // We configure the size of default buffer to be the size of camera preview we want.
                texture.SetDefaultBufferSize(PreviewSize.Width, PreviewSize.Height);

                // This is the output Surface we need to start preview.
                Surface surface = new Surface(texture);

                // We set up a CaptureRequest.Builder with the output Surface.
                PreviewRequestBuilder = Device.CreateCaptureRequest(CameraTemplate.Preview);
                PreviewRequestBuilder.AddTarget(surface);

                // Here, we create a CameraCaptureSession for camera preview.
                Device.CreateCaptureSession(new List<Surface>()
                {
                    surface,
                    JpegImageReader.Get ().Surface,
                    RawImageReader.Get ().Surface
                }, 
                new CameraPreviewCaptureCallback(OnCameraPreviewCaptureConfigured, OnShowToast), 
                BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        /// <summary>
        /// Configure the given {@link CaptureRequest.Builder} to use auto-focus, auto-exposure, and
        /// auto-white-balance controls if available.
        /// 
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        /// <param name="builder">the builder to configure.</param>
        private void Setup3AControlsLocked(CaptureRequest.Builder builder)
        {
            // Enable auto-magical 3A run by camera device
            builder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);

            var minFocusDist = (float)Characteristics.Get(CameraCharacteristics.LensInfoMinimumFocusDistance);

            // If MINIMUM_FOCUS_DISTANCE is 0, lens is fixed-focus and we need to skip the AF run.
            NoAFRun = (minFocusDist == null || minFocusDist == 0);

            if (!NoAFRun)
            {
                // If there is a "continuous picture" mode available, use it, otherwise default to AUTO.
                if (Contains(Characteristics.Get(CameraCharacteristics.ControlAfAvailableModes).ToArray<int>(), (int)ControlAFMode.ContinuousPicture))
                {
                    builder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
                }
                else
                {
                    builder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.Auto);
                }
            }

            // If there is an auto-magical flash control mode available, use it, otherwise default to
            // the "on" mode, which is guaranteed to always be available.
            if (Contains(Characteristics.Get(CameraCharacteristics.ControlAeAvailableModes).ToArray<int>(), (int)ControlAEMode.OnAutoFlash))
            {
                builder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
            }
            else
            {
                builder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
            }

            // If there is an auto-magical white balance control mode available, use it.
            if (Contains(Characteristics.Get(CameraCharacteristics.ControlAwbAvailableModes).ToArray<int>(), (int)ControlAwbMode.Auto))
            {
                // Allow AWB to run auto-magically if this device supports this
                builder.Set(CaptureRequest.ControlAwbMode, (int)ControlAwbMode.Auto);
            }
        }

        /// <summary>
        /// Send a capture request to the camera device that initiates a capture targeting the JPEG and
        /// RAW outputs.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        private void CaptureStillPictureLocked(Activity activity)
        {
            try
            {
                if (null == activity || null == Device)
                    return;

                // This is the CaptureRequest.Builder that we use to take a picture.
                CaptureRequest.Builder captureBuilder =
                    Device.CreateCaptureRequest(CameraTemplate.StillCapture);

                captureBuilder.AddTarget(JpegImageReader.Get().Surface);
                captureBuilder.AddTarget(RawImageReader.Get().Surface);

                // Use the same AE and AF modes as the preview.
                Setup3AControlsLocked(captureBuilder);

                // Set orientation.
                var rotation = activity.WindowManager.DefaultDisplay.Rotation;
                captureBuilder.Set(CaptureRequest.JpegOrientation, SensorToDeviceRotation(Characteristics, (int)rotation));

                // Set request tag to easily track results in callbacks.
                captureBuilder.SetTag(RequestCounter.IncrementAndGet());

                CaptureRequest request = captureBuilder.Build();

                // Create an ImageSaverBuilder in which to collect results, and add it to the queue
                // of active requests.
                ImageSaverBuilder jpegBuilder = new ImageSaverBuilder(activity)
                    .SetCharacteristics(Characteristics);
                ImageSaverBuilder rawBuilder = new ImageSaverBuilder(activity)
                    .SetCharacteristics(Characteristics);

                JpegResultQueue.Put((int)request.Tag, jpegBuilder);
                RawResultQueue.Put((int)request.Tag, rawBuilder);

                CaptureSession.Capture(request, CaptureCallback, BackgroundHandler);

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        /// <summary>
        /// Called after a RAW/JPEG capture has completed; resets the AF trigger state for the
        /// pre-capture sequence.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        private void FinishedCaptureLocked()
        {
            try
            {
                // Reset the auto-focus trigger in case AF didn't run quickly enough.
                if (!NoAFRun)
                {
                    PreviewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);

                    CaptureSession.Capture(PreviewRequestBuilder.Build(), PreCaptureCallback, BackgroundHandler);

                    PreviewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Idle);
                }
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        /// <summary>
        /// Return true if the given array contains the given integer.
        /// </summary>
        /// <returns><c>true</c>, if the array contains the given integer, <c>false</c> otherwise.</returns>
        /// <param name="modes">array to check.</param>
        /// <param name="mode">integer to get for.</param>
        private bool Contains(int[] modes, int mode)
        {
            if (modes == null)
            {
                return false;
            }
            foreach (int i in modes)
            {
                if (i == mode)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the given request has been completed, remove it from the queue of active requests and
        /// send an {@link ImageSaver} with the results from this request to a background thread to
        /// save a file.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        /// <param name="requestId">the ID of the {@link CaptureRequest} to handle.</param>
        /// <param name="builder">the {@link ImageSaver.ImageSaverBuilder} for this request.</param>
        /// <param name="queue">the queue to remove this request from, if completed.</param>
        private void HandleCompletionLocked(int requestId, ImageSaverBuilder builder, TreeMap queue)
        {
            if (builder == null)
                return;

            ImageSaver saver = builder.buildIfComplete();
            if (saver != null)
            {
                queue.Remove(requestId);
                AsyncTask.ThreadPoolExecutor.Execute(saver);
            }
        }

        /// <summary>
        /// Check if we are using a device that only supports the LEGACY hardware level.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        /// <returns><c>true</c> if this is a legacy device; otherwise, <c>false</c>.</returns>
        private bool IsLegacyLocked()
        {
            return (int)Characteristics.Get(CameraCharacteristics.InfoSupportedHardwareLevel) == (int)InfoSupportedHardwareLevel.Legacy;
        }

        /// <summary>
        /// Check if the timer for the pre-capture sequence has been hit.
        ///
        /// Call this only with {@link #mCameraStateLock} held.
        /// </summary>
        /// <returns><c>true</c>, if the timeout occurred, <c>false</c> otherwise.</returns>
        private bool HitTimeoutLocked()
        {
            return (SystemClock.ElapsedRealtime() - CaptureTimer) > PRECAPTURE_TIMEOUT_MS;
        }

        /// <summary>
        /// Generate a string containing a formatted timestamp with the current date and time.
        /// </summary>
        /// <returns>a {@link String} representing a time.</returns>
        private string GenerateTimestamp()
        {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy_MM_dd_HH_mm_ss_SSS", Java.Util.Locale.Uk);
            return sdf.Format(new Date());
        }

        /// <summary>
        /// Rotation need to transform from the camera sensor orientation to the device's current
        /// orientation.
        /// </summary>
        /// <returns>the total rotation from the sensor orientation to the current device orientation.</returns>
        /// <param name="c">the {@link CameraCharacteristics} to query for the camera sensor orientation.</param>
        /// <param name="deviceOrientation">the current device orientation relative to the native device orientation.</param>
        private int SensorToDeviceRotation(CameraCharacteristics c, int deviceOrientation)
        {
            int sensorOrientation = (int)c.Get(CameraCharacteristics.SensorOrientation);

            // Get device orientation in degrees
            deviceOrientation = ORIENTATIONS.Get(deviceOrientation);

            // Reverse device orientation for front-facing cameras
            if ((int)c.Get(CameraCharacteristics.LensFacing) == (int)LensFacing.Front)
            {
                deviceOrientation = -deviceOrientation;
            }

            // Calculate desired JPEG orientation relative to camera orientation to make
            // the image upright relative to the device orientation
            return (sensorOrientation + deviceOrientation + 360) % 360;
        }

        /// <summary>
        /// Given {@code choices} of {@code Size}s supported by a camera, chooses the smallest one whose
        /// width and height are at least as large as the respective requested values, and whose aspect
        /// ratio matches with the specified value.
        /// </summary>
        /// <returns>The optimal {@code Size}, or an arbitrary one if none were big enough</returns>
        /// <param name="choices">The list of sizes that the camera supports for the intended output class</param>
        /// <param name="width">The minimum desired width</param>
        /// <param name="height">The minimum desired height</param>
        /// <param name="aspectRatio">The aspect ratio</param>
        private Size ChooseOptimalSize(Size[] choices, int width, int height, Size aspectRatio)
        {
            // Collect the supported resolutions that are at least as big as the preview Surface
            List<Size> bigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;
            foreach (Size option in choices)
            {
                if (option.Height == option.Width * h / w &&
                    option.Width >= width && option.Height >= height)
                {
                    bigEnough.Add(option);
                }
            }

            // Pick the smallest of those, assuming we found any
            if (bigEnough.Count > 0)
            {
                return (Size)Collections.Min(bigEnough, new CompareSizesByArea());
            }
            else
            {
                Log.Error("DroidCameraService", "Couldn't find any suitable preview size");
                return choices[0];
            }
        }

        /// <summary>
        /// Return true if the two given {@link Size}s have the same aspect ratio.
        /// </summary>
        /// <returns><c>true</c>, if the sizes have the same aspect ratio, <c>false</c> otherwise.</returns>
        /// <param name="a">first {@link Size} to compare.</param>
        /// <param name="b">second {@link Size} to compare.</param>
        private bool CheckAspectsEqual(Size a, Size b)
        {
            double aAspect = a.Width / (double)a.Height;
            double bAspect = b.Width / (double)b.Height;
            return System.Math.Abs(aAspect - bAspect) <= ASPECT_RATIO_TOLERANCE;
        }

        /// <summary>
        /// Sets up state related to camera that is needed before opening a {@link CameraDevice}.
        /// </summary>
        /// <returns><c>true</c>, if up camera outputs was set, <c>false</c> otherwise.</returns>
        private bool SetUpCameraOutputs()
        {
            var activity = CurrentActivity;
            CameraManager manager = (CameraManager)activity.GetSystemService(Context.CameraService);
            if (manager == null)
            {
                ErrorDialog.BuildErrorDialog("This device doesn't support Camera2 API.").Show(CurrentFragmentManager, "dialog");
                return false;
            }
            try
            {
                // Find a CameraDevice that supports RAW captures, and configure state.
                foreach (string cameraId in manager.GetCameraIdList())
                {
                    CameraCharacteristics characteristics
                    = manager.GetCameraCharacteristics(cameraId);

                    // We only use a camera that supports RAW in this sample.
                    if (!Contains(characteristics.Get(
                            CameraCharacteristics.RequestAvailableCapabilities).ToArray<int>(),
                            (int)RequestAvailableCapabilities.Raw))
                    {
                        continue;
                    }

                    StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(
                                                     CameraCharacteristics.ScalerStreamConfigurationMap);

                    // For still image captures, we use the largest available size.
                    Size[] jpegs = map.GetOutputSizes((int)ImageFormatType.Jpeg);
                    Size largestJpeg = jpegs.OrderByDescending(element => element.Width * element.Height).First();

                    Size[] raws = map.GetOutputSizes((int)ImageFormatType.RawSensor);
                    Size largestRaw = raws.OrderByDescending(element => element.Width * element.Height).First();

                    lock (CameraStateLock)
                    {
                        // Set up ImageReaders for JPEG and RAW outputs.  Place these in a reference
                        // counted wrapper to ensure they are only closed when all background tasks
                        // using them are finished.
                        if (JpegImageReader == null || JpegImageReader.GetAndRetain() == null)
                        {
                            JpegImageReader = new RefCountedAutoCloseable<ImageReader>(
                                ImageReader.NewInstance(largestJpeg.Width,
                                    largestJpeg.Height, ImageFormatType.Jpeg, /*maxImages*/5));
                        }

                        JpegImageReader.Get().SetOnImageAvailableListener(
                            OnJpegImageAvailableListener, BackgroundHandler);

                        if (RawImageReader == null || RawImageReader.GetAndRetain() == null)
                        {
                            RawImageReader = new RefCountedAutoCloseable<ImageReader>(
                                ImageReader.NewInstance(largestRaw.Width,
                                    largestRaw.Height, ImageFormatType.RawSensor, /*maxImages*/5));
                        }
                        RawImageReader.Get().SetOnImageAvailableListener(
                            OnRawImageAvailableListener, BackgroundHandler);

                        Characteristics = characteristics;
                        CameraId = cameraId;
                    }
                    return true;
                }
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }

            // If we found no suitable cameras for capturing RAW, warn the user.
            ErrorDialog.BuildErrorDialog("This device doesn't support capturing RAW photos").Show(CurrentFragmentManager, "dialog");
            return false;
        }

        /// <summary>
        /// Opens the camera specified by {@link #mCameraId}.
        /// </summary>
        private bool CanOpenCamera()
        {
            if (!SetUpCameraOutputs())
                return false;

            var activity = CurrentActivity;
            CameraManager manager = (CameraManager)activity.GetSystemService(Context.CameraService);
            try
            {
                // Wait for any previously running session to finish.
                if (!CameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                    throw new RuntimeException("Time out waiting to lock camera opening.");

                string cameraId;
                Handler backgroundHandler;
                lock (CameraStateLock)
                {
                    cameraId = CameraId;
                    backgroundHandler = BackgroundHandler;
                }

                // Attempt to open the camera. mStateCallback will be called on the background handler's
                // thread when this succeeds or fails.
                manager.OpenCamera(cameraId, StateCallback, backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.", e);
            }
            return true;
        }

        /// <summary>
        /// Closes the current {@link CameraDevice}.
        /// </summary>
        private void CloseCamera()
        {
            try
            {
                CameraOpenCloseLock.Acquire();
                lock (CameraStateLock)
                {

                    // Reset state and clean up resources used by the camera.
                    // Note: After calling this, the ImageReaders will be closed after any background
                    // tasks saving Images from these readers have been completed.
                    PendingUserCaptures = 0;
                    State = CameraStateEnum.STATE_CLOSED;
                    if (null != CaptureSession)
                    {
                        CaptureSession.Close();
                        CaptureSession = null;
                    }
                    if (null != Device)
                    {
                        Device.Close();
                        Device = null;
                    }
                    if (null != JpegImageReader)
                    {
                        JpegImageReader.Close();
                        JpegImageReader = null;
                    }
                    if (null != RawImageReader)
                    {
                        RawImageReader.Close();
                        RawImageReader = null;
                    }
                }
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera closing.", e);
            }
            finally
            {
                CameraOpenCloseLock.Release();
            }
        }

        /// <summary>
        /// Starts a background thread and its {@link Handler}.
        /// </summary>
        private void StartBackgroundThread()
        {
            BackgroundThread = new HandlerThread("CameraBackground");
            BackgroundThread.Start();
            lock (CameraStateLock)
            {
                BackgroundHandler = new Handler(BackgroundThread.Looper);
            }
        }

        /// <summary>
        /// Stops the background thread and its {@link Handler}.
        /// </summary>
        private void StopBackgroundThread()
        {
            BackgroundThread.QuitSafely();
            try
            {
                BackgroundThread.Join();
                BackgroundThread = null;
                lock (CameraStateLock)
                {
                    BackgroundHandler = null;
                }
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }

        #endregion
    }
}