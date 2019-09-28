using System;

namespace Better_xNet
{
    public sealed class MultiThreadingRepeatEventArgs : EventArgs
    {
        public ulong RepeatCount
        {
            get;
            private set;
        }

        public MultiThreadingRepeatEventArgs(ulong repeatCount)
        {
            RepeatCount = repeatCount;
        }
    }
}
