#include "MobileLib.h"
#include <jni.h>
#include <string>
#include <android/log.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>

#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, "Camera2Demo", __VA_ARGS__)

char * AndroidInfo()
{
	return MobileLib::getTemplateInfo();
}

extern "C" {

	JNIEXPORT jstring JNICALL MvvmCrossTest_Core_Droid_Helper_JNIUtils_AndroidInfo(
		JNIEnv *env,
		jobject obj)
	{
		jstring result;
		char* data = MobileLib::getTemplateInfo();

		LOGE("JNI: %s\n", data);

		result = env->NewStringUTF(data);

		return result;
	}

	JNIEXPORT void JNICALL  MvvmCrossTest_Core_Droid_Helper_JNIUtils_GrayscaleMOTOGDisplay(
		JNIEnv *env,
		jobject obj,
		jobject srcBuffer,
		jobject surface) {

		uint8_t *srcLumaPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(srcBuffer));

		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;

		size_t w = 1280;
		size_t h = 960;

		ANativeWindow_setBuffersGeometry(window, w, h, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}

		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);

		for (size_t y = 0, dy = h - 1; y < h; y++, dy--)
		{
			for (size_t x = 0, dx = w - 1; x < w; x++, dx--)
			{
				uint8_t * rowPtr = srcLumaPtr + (dx * w) + y;
				*(outPtr++) = *rowPtr; //R
				*(outPtr++) = *rowPtr; //G
				*(outPtr++) = *rowPtr; //B
				*(outPtr++) = 255; // gamma for RGBA_8888
			}
		}

		ANativeWindow_unlockAndPost(window);
		ANativeWindow_release(window);
	}

	JNIEXPORT void JNICALL  MvvmCrossTest_Core_Droid_Helper_JNIUtils_GrayscaleDisplay(
		JNIEnv *env,
		jobject obj,
		jint srcWidth,
		jint srcHeight,
		jint rowStride,
		jobject srcBuffer,
		jobject surface) {

		uint8_t *srcLumaPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(srcBuffer));

		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;

		ANativeWindow_setBuffersGeometry(window, srcWidth, srcHeight, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}


		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);

		for (size_t y = 0; y < srcHeight; ++y)
		{
			uint8_t * rowPtr = srcLumaPtr + y * rowStride;
			for (size_t x = 0; x < srcWidth; x++)
			{
				//for grayscale output, just duplicate the Y channel into R, G, B channels
				*(outPtr++) = *rowPtr; //R
				*(outPtr++) = *rowPtr; //G
				*(outPtr++) = *rowPtr; //B
				*(outPtr++) = 255; // gamma for RGBA_8888
				++rowPtr;
			}
		}

		ANativeWindow_unlockAndPost(window);
		ANativeWindow_release(window);
	}

	JNIEXPORT void JNICALL  MvvmCrossTest_Core_Droid_Helper_JNIUtils_GrayscaleDisplayCcw90(
		JNIEnv *env,
		jobject obj,
		jint srcWidth,
		jint srcHeight,
		jint rowStride,
		jobject srcBuffer,
		jobject surface) {

		uint8_t *srcLumaPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(srcBuffer));

		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;

		ANativeWindow_setBuffersGeometry(window, srcWidth, srcHeight, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}


		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);

		for (size_t y = 0, dy = srcHeight - 1; y < srcHeight; y++, dy--)
		{
			for (size_t x = 0, dx = srcWidth - 1; x < srcWidth; x++, dx--)
			{
				uint8_t * rowPtr = srcLumaPtr + (dx * srcWidth) + y;
				*(outPtr++) = *rowPtr; //R
				*(outPtr++) = *rowPtr; //G
				*(outPtr++) = *rowPtr; //B
				*(outPtr++) = 255; // gamma for RGBA_8888
			}
		}

		ANativeWindow_unlockAndPost(window);
		ANativeWindow_release(window);
	}

	//do YUV_420_88 to RGBA_8888 conversion, flip, and display
	JNIEXPORT void JNICALL MvvmCrossTest_Core_Droid_Helper_JNIUtils_RGBADisplay(
		JNIEnv *env,
		jobject obj,
		jint srcWidth,
		jint srcHeight,
		jint Y_rowStride,
		jobject Y_Buffer,
		jint UV_rowStride,
		jobject U_Buffer,
		jobject V_Buffer,
		jobject surface) {

		uint8_t *srcYPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(Y_Buffer));
		uint8_t *srcUPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(U_Buffer));
		uint8_t *srcVPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(V_Buffer));
		/*
		if (srcYPtr == nullptr)
		{
		LOGE("srcYPtr null ERROR!");
		return;
		}
		else if (srcUPtr == nullptr)
		{
		LOGE("srcUPtr null ERROR!");
		return;
		}
		else if (srcVPtr == nullptr)
		{
		LOGE("srcVPtr null ERROR!");
		return;
		}
		*/
		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;
		//set output size and format
		//only 3 formats are available:
		//WINDOW_FORMAT_RGBA_8888(DEFAULT), WINDOW_FORMAT_RGBX_8888, WINDOW_FORMAT_RGB_565
		ANativeWindow_setBuffersGeometry(window, srcWidth, srcHeight, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}

		size_t bufferSize = buffer.width * buffer.height * (size_t)4;

		//YUV_420_888 to RGBA_8888 conversion and flip
		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);
		for (size_t y = 0; y < srcHeight; y++)
		{
			uint8_t * Y_rowPtr = srcYPtr + y * Y_rowStride;
			uint8_t * U_rowPtr = srcUPtr + (y >> 1) * UV_rowStride;
			uint8_t * V_rowPtr = srcVPtr + (y >> 1) * UV_rowStride;
			for (size_t x = 0; x < srcWidth; x++)
			{
				uint8_t Y = Y_rowPtr[x];
				uint8_t U = U_rowPtr[(x >> 1)];
				uint8_t V = V_rowPtr[(x >> 1)];
				//from Wikipedia article YUV:
				//Integer operation of ITU-R standard for YCbCr(8 bits per channel) to RGB888
				//Y-Y, U-Cb, V-Cr
				//U -= 128
				//V -= 128
				//R = Y + V + (V >> 2) + (V >> 3) + (V >> 5)
				//  = Y + V * 1.40625;
				//G = Y - ((U >> 2) + (U >> 4) + (U >> 5)) - ((V >> 1) + (V >> 3) + (V >> 4) + (V >> 5))
				//  = Y - (U - 128) * 0.34375 - (V - 128) * 0.71875;
				//B = Y + U + (U >> 1) + (U >> 2) + (U >> 6)
				//  = Y + (U - 128) * 1.765625;
				double R = (Y + (V - 128) * 1.40625);
				double G = (Y - (U - 128) * 0.34375 - (V - 128) * 0.71875);
				double B = (Y + (U - 128) * 1.765625);
				*(outPtr + (--bufferSize)) = 255; // gamma for RGBA_8888
				*(outPtr + (--bufferSize)) = (uint8_t)(B > 255 ? 255 : (B < 0 ? 0 : B));
				*(outPtr + (--bufferSize)) = (uint8_t)(G > 255 ? 255 : (G < 0 ? 0 : G));
				*(outPtr + (--bufferSize)) = (uint8_t)(R > 255 ? 255 : (R < 0 ? 0 : R));
			}
		}

		ANativeWindow_unlockAndPost(window);
		ANativeWindow_release(window);
	}

	//using another conversion method offered by @alijandro at a question in stackoverflow
	//https://stackoverflow.com/questions/46087343/jni-yuv-420-888-to-rgba-8888-conversion
	JNIEXPORT void JNICALL MvvmCrossTest_Core_Droid_Helper_JNIUtils_RGBADisplay2(
		JNIEnv *env,
		jobject obj,
		jint srcWidth,
		jint srcHeight,
		jint Y_rowStride,
		jobject Y_Buffer,
		jobject U_Buffer,
		jobject V_Buffer,
		jobject surface) {

		uint8_t *srcYPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(Y_Buffer));
		uint8_t *srcUPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(U_Buffer));
		uint8_t *srcVPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(V_Buffer));
		/*
		if (srcYPtr == nullptr)
		{
		LOGE("srcYPtr null ERROR!");
		return;
		}
		else if (srcUPtr == nullptr)
		{
		LOGE("srcUPtr null ERROR!");
		return;
		}
		else if (srcVPtr == nullptr)
		{
		LOGE("srcVPtr null ERROR!");
		return;
		}
		*/
		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;
		//set output size and format
		//only 3 formats are available:
		//WINDOW_FORMAT_RGBA_8888(DEFAULT), WINDOW_FORMAT_RGBX_8888, WINDOW_FORMAT_RGB_565
		ANativeWindow_setBuffersGeometry(window, srcWidth, srcHeight, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}

		size_t bufferSize = buffer.width * buffer.height * (size_t)4;

		//YUV_420_888 to RGBA_8888 conversion
		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);
		for (size_t y = 0; y < srcHeight; y++)
		{
			uint8_t * Y_rowPtr = srcYPtr + y * Y_rowStride;
			uint8_t * U_rowPtr = srcUPtr + (y >> 1) * Y_rowStride / 2;
			uint8_t * V_rowPtr = srcVPtr + (y >> 1) * Y_rowStride / 2;
			for (size_t x = 0; x < srcWidth; x++)
			{
				//from Wikipedia article YUV:
				//Integer operation of ITU-R standard for YCbCr(8 bits per channel) to RGB888
				//Y-Y, U-Cb, V-Cr
				//R = Y + V + (V >> 2) + (V >> 3) + (V >> 5);
				//G = Y - ((U >> 2) + (U >> 4) + (U >> 5)) - ((V >> 1) + (V >> 3) + (V >> 4) + (V >> 5));
				//B = Y + U + (U >> 1) + (U >> 2) + (U >> 6);
				uint8_t Y = Y_rowPtr[x];
				uint8_t U = U_rowPtr[(x >> 1)];
				uint8_t V = V_rowPtr[(x >> 1)];
				double R = ((Y - 16) * 1.164 + (V - 128) * 1.596);
				double G = ((Y - 16) * 1.164 - (U - 128) * 0.392 - (V - 128) * 0.813);
				double B = ((Y - 16) * 1.164 + (U - 128) * 2.017);
				*(outPtr + (--bufferSize)) = 255; // gamma for RGBA_8888
				*(outPtr + (--bufferSize)) = (uint8_t)(B > 255 ? 255 : (B < 0 ? 0 : B));
				*(outPtr + (--bufferSize)) = (uint8_t)(G > 255 ? 255 : (G < 0 ? 0 : G));
				*(outPtr + (--bufferSize)) = (uint8_t)(R > 255 ? 255 : (R < 0 ? 0 : R));
			}
		}

		ANativeWindow_unlockAndPost(window);
		ANativeWindow_release(window);
	}

	JNIEXPORT void JNICALL MvvmCrossTest_Core_Droid_Helper_JNIUtils_FlexRGBADisplay(
		JNIEnv *env,
		jobject obj,
		jint srcWidth,
		jint srcHeight,
		jint rowStride,
		jobject R_Buffer,
		jobject G_Buffer,
		jobject B_Buffer,
		jobject A_Buffer,
		jobject surface) {

		uint8_t *srcRPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(R_Buffer));
		uint8_t *srcGPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(G_Buffer));
		uint8_t *srcBPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(B_Buffer));
		uint8_t *srcAPtr = reinterpret_cast<uint8_t *>(env->GetDirectBufferAddress(A_Buffer));

		ANativeWindow * window = ANativeWindow_fromSurface(env, surface);
		ANativeWindow_acquire(window);
		ANativeWindow_Buffer buffer;

		ANativeWindow_setBuffersGeometry(window, srcWidth, srcHeight, WINDOW_FORMAT_RGBA_8888);
		if (int32_t err = ANativeWindow_lock(window, &buffer, NULL)) {
			LOGE("ANativeWindow_lock failed with error code: %d\n", err);
			ANativeWindow_release(window);
		}

		//FlexRGBA_8888 to RGBA_8888 (direct passing of data)
		uint8_t * outPtr = reinterpret_cast<uint8_t *>(buffer.bits);
		for (size_t y = 0; y < srcHeight; y++)
		{
			uint8_t * rowRPtr = srcRPtr + y * rowStride;
			uint8_t * rowGPtr = srcGPtr + y * rowStride;
			uint8_t * rowBPtr = srcBPtr + y * rowStride;
			uint8_t * rowAPtr = srcAPtr + y * rowStride;
			for (size_t x = 0; x < srcWidth; x++)
			{
				*(outPtr++) = *rowRPtr; //R
				*(outPtr++) = *rowGPtr; //G
				*(outPtr++) = *rowBPtr; //B
				*(outPtr++) = *rowAPtr; //A

				++rowRPtr;
				++rowGPtr;
				++rowBPtr;
				++rowAPtr;
			}
		}
	}
}