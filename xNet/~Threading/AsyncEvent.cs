using System;
using System.ComponentModel;
using System.Threading;

namespace Better_xNet
{
    public class AsyncEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly Action<TEventArgs> _onEvent;

        private readonly SendOrPostCallback _callbackOnEvent;

        public EventHandler<TEventArgs> EventHandler
        {
            get;
            set;
        }

        public AsyncEvent(Action<TEventArgs> onEvent)
        {
            if (onEvent == null)
            {
                throw new ArgumentNullException("onEvent");
            }
            _onEvent = onEvent;
            _callbackOnEvent = OnCallback;
        }

        public void On(object sender, TEventArgs eventArgs)
        {
            EventHandler?.Invoke(sender, eventArgs);
        }

        public void Post(AsyncOperation asyncOperation, object sender, TEventArgs eventArgs)
        {
            if (asyncOperation == null)
            {
                On(sender, eventArgs);
            }
            else
            {
                asyncOperation.Post(_callbackOnEvent, eventArgs);
            }
        }

        public void PostOperationCompleted(AsyncOperation asyncOperation, object sender, TEventArgs eventArgs)
        {
            if (asyncOperation == null)
            {
                On(sender, eventArgs);
            }
            else
            {
                asyncOperation.PostOperationCompleted(_callbackOnEvent, eventArgs);
            }
        }

        private void OnCallback(object param)
        {
            _onEvent(param as TEventArgs);
        }
    }
}
