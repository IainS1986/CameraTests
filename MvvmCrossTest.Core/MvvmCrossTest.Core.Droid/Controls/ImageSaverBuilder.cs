using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace MvvmCrossTest.Core.Droid.Controls
{
    /// <summary>
    /// Builder class for constructing {@link ImageSaver}s.
    ///
    /// This class is thread safe.
    /// </summary>
    public class ImageSaverBuilder : Java.Lang.Object
    {
        Image mImage;
        FileInfo mFile;
        CaptureResult mCaptureResult;
        CameraCharacteristics mCharacteristics;
        Context mContext;
        RefCountedAutoCloseable<ImageReader> mReader;

        /// <summary>
        /// Construct a new ImageSaverBuilder using the given {@link Context}.
        /// @param context a {@link Context} to for accessing the
        ///                  {@link android.provider.MediaStore}.
        /// </summary>
        public ImageSaverBuilder(Context context)
        {
            mContext = context;
        }

        public ImageSaverBuilder SetRefCountedReader(
            RefCountedAutoCloseable<ImageReader> reader)
        {
            if (reader == null)
                throw new NullPointerException();

            mReader = reader;
            return this;
        }

        public ImageSaverBuilder SetImage(Image image)
        {
            if (image == null)
                throw new NullPointerException();
            mImage = image;
            return this;
        }

        public ImageSaverBuilder SetFile(FileInfo file)
        {
            if (file == null)
                throw new NullPointerException();
            mFile = file;
            return this;
        }

        public ImageSaverBuilder SetResult(CaptureResult result)
        {
            if (result == null)
                throw new NullPointerException();
            mCaptureResult = result;
            return this;
        }

        public ImageSaverBuilder SetCharacteristics(
            CameraCharacteristics characteristics)
        {
            if (characteristics == null)
                throw new NullPointerException();
            mCharacteristics = characteristics;
            return this;
        }

        public ImageSaver buildIfComplete()
        {
            if (!IsComplete)
            {
                return null;
            }
            return new ImageSaver(mImage, mFile, mCaptureResult, mCharacteristics, mContext, mReader);
        }

        public string GetSaveLocation()
        {
            return (mFile == null) ? "Unknown" : mFile.ToString();
        }

        bool IsComplete
        {
            get
            {
                return mImage != null && mFile != null && mCaptureResult != null
                && mCharacteristics != null;
            }
        }
    }
}