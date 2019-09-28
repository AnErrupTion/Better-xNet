using System;

namespace Better_xNet
{
    public sealed class MultiThreadingProgressEventArgs : EventArgs
    {
        public object Result
        {
            get;
            private set;
        }

        public MultiThreadingProgressEventArgs(object result)
        {
            Result = result;
        }
    }
}
