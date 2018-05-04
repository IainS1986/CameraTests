using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.FastAndroidCamera
{
    /// <summary>
    /// https://github.com/jamesathey/FastAndroidCamera/blob/master/FastJavaByteArrayEnumerator.cs
    /// </summary>
    internal class FastJavaByteArrayEnumerator : IEnumerator<byte>
    {
        internal FastJavaByteArrayEnumerator(FastJavaByteArray arr)
        {
            if (arr == null)
                throw new ArgumentNullException();

            _arr = arr;
            _idx = 0;
        }

        /// <summary>
        /// Gets the current byte in the collection.
        /// </summary>
        public byte Current
        {
            get
            {
                byte retval;
                unsafe
                {
                    // get value from pointer
                    retval = _arr.Raw[_idx];
                }
                return retval;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:ApxLabs.FastAndroidCamera.FastJavaByteArrayEnumerator"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:ApxLabs.FastAndroidCamera.FastJavaByteArrayEnumerator"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:ApxLabs.FastAndroidCamera.FastJavaByteArrayEnumerator"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:ApxLabs.FastAndroidCamera.FastJavaByteArrayEnumerator"/> so the garbage collector can reclaim the
        /// memory that the <see cref="T:ApxLabs.FastAndroidCamera.FastJavaByteArrayEnumerator"/> was occupying.</remarks>
        public void Dispose()
        {
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            if (_idx > _arr.Count)
                return false;

            ++_idx;

            return _idx < _arr.Count;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _idx = 0;
        }

        #region IEnumerator implementation

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <value>The system. collections. IE numerator. current.</value>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                byte retval;
                unsafe
                {
                    // get value from pointer
                    retval = _arr.Raw[_idx];
                }
                return retval;
            }
        }

        #endregion

        FastJavaByteArray _arr;
        int _idx;
    }
}