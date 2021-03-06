﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ApxLabs.FastAndroidCamera;

namespace MvvmCrossTest.Core.Droid.Helper
{
    public class GraphicsHelper
    {
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

        public static void applyGrayScaleAndRotate90(int[] pixels, FastJavaByteArray data, int width, int height)
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