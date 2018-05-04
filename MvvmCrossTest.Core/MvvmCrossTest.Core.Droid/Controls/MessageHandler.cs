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

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class MessageHandler : Handler
    {
        private Action<Message> OnMessage;

        public MessageHandler(Looper looper, Action<Message> onMessage) : base(looper)
        {
            OnMessage = onMessage;
        }

        public override void HandleMessage(Message msg)
        {
            if (OnMessage != null)
                OnMessage(msg);
        }
    }
}