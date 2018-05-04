using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Nio;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class ImageSaver : Java.Lang.Object, IRunnable
    {
        /// <summary>
        /// The image to save.
        /// </summary>
        readonly Image mImage;

        /// <summary>
        /// The file we save the image into.
        /// </summary>
        readonly FileInfo mFile;

        /// <summary>
        /// The CaptureResult for this image capture.
        /// </summary>
        readonly CaptureResult mCaptureResult;

        /// <summary>
        /// The CameraCharacteristics for this camera device.
        /// </summary>
        readonly CameraCharacteristics mCharacteristics;

        /// <summary>
        /// The Context to use when updating MediaStore with the saved images.
        /// </summary>
        readonly Context mContext;

        /// <summary>
        /// A reference counted wrapper for the ImageReader that owns the given image.
        /// </summary>
        readonly RefCountedAutoCloseable<ImageReader> mReader;

        public ImageSaver(Image image,
                        FileInfo file, 
                        CaptureResult result,
                        CameraCharacteristics characteristics,
                        Context context,
                        RefCountedAutoCloseable<ImageReader> reader)
        {
            mImage = image;
            mFile = file;
            mCaptureResult = result;
            mCharacteristics = characteristics;
            mContext = context;
            mReader = reader;
        }

        public void Run()
        {
            bool success = false;
            var format = mImage.Format;
            switch (format)
            {
                case ImageFormatType.Jpeg:
                    {
                        ByteBuffer buffer = mImage.GetPlanes()[0].Buffer;
                        byte[] bytes = new byte[buffer.Remaining()];
                        buffer.Get(bytes);
                        FileStream output = null;
                        try
                        {
                            output = mFile.OpenWrite();
                            output.Write(bytes, 0, bytes.Length);
                            success = true;
                        }
                        catch (IOException e)
                        {
                            Log.Error("ImageSaver", e.Message);
                        }
                        finally
                        {
                            mImage.Close();
                            CloseOutput(output);
                        }
                        break;
                    }
                case ImageFormatType.RawSensor:
                    {
                        DngCreator dngCreator = new DngCreator(mCharacteristics, mCaptureResult);
                        FileStream output = null;
                        try
                        {
                            output = mFile.OpenWrite();
                            dngCreator.WriteImage(output, mImage);
                            success = true;
                        }
                        catch (IOException e)
                        {
                            Log.Error("ImageSaver", e.Message);
                        }
                        finally
                        {
                            mImage.Close();
                            CloseOutput(output);
                        }
                        break;
                    }
                default:
                    {
                        Log.Error("ImageSaver", "Cannot save image, unexpected image format:" + format);
                        break;
                    }
            }

            // Decrement reference count to allow ImageReader to be closed to free up resources.
            mReader.Close();

            // If saving the file succeeded, update MediaStore.
            if (success)
            {
                MediaScannerConnection.ScanFile(mContext, new string[] { mFile.FullName },
                    /*mimeTypes*/null, new MediaScannerClient());
            }
        }

        /// <summary>
        /// Cleanup the given {@link OutputStream}.
        /// </summary>
        /// <param name="outputStream">the stream to close.</param>
        private void CloseOutput(System.IO.Stream outputStream)
        {
            if (outputStream != null)
            {
                try
                {
                    outputStream.Close();
                }
                catch (IOException e)
                {
                    Log.Error("ImageSaver", e.Message);
                }
            }
        }
    }
}