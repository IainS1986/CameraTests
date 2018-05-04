using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.Controls
{
    /// <summary>
    /// A dialog fragment for displaying non-recoverable errors; this {@link Activity} will be
    /// finished once the dialog has been acknowledged by the user. 
    /// </summary>
    public class ErrorDialog : DialogFragment
    {

        string mErrorMessage;

        public ErrorDialog()
        {
            mErrorMessage = "Unknown error occurred!";
        }

        // Build a dialog with a custom message (Fragments require default constructor).
        public static ErrorDialog BuildErrorDialog(string errorMessage)
        {
            ErrorDialog dialog = new ErrorDialog();
            dialog.mErrorMessage = errorMessage;
            return dialog;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Dialogue, container);
            TextView text = (TextView)view.FindViewById(Resource.Id.txt_dia);
            text.Text = mErrorMessage;
            // This shows the title, replace My Dialog Title. You can use strings too.
            Dialog.SetTitle("Error");
            // If you want no title, use this code
            // getDialog().getWindow().requestFeature(Window.FEATURE_NO_TITLE);

            return view;
        }
    }
}