using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Better_xNet
{
    [SuppressUnmanagedCodeSecurityAttribute]
    [Obsolete("This class is obsolete since the creation of newer browsers, please use the other classes! (HttpRequest, Http, etc...)", true)]
    internal static class SafeNativeMethods
    {
        [Flags]
        internal enum InternetConnectionState : int
        {
            INTERNET_CONNECTION_MODEM = 0x1,
            INTERNET_CONNECTION_LAN = 0x2,
            INTERNET_CONNECTION_PROXY = 0x4,
            INTERNET_RAS_INSTALLED = 0x10,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_CONFIGURED = 0x40
        }


        [DllImport("wininet.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool InternetGetConnectedState(
            ref InternetConnectionState lpdwFlags, int dwReserved);
    }
}
