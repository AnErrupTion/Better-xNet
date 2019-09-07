using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Better_xNet
{
    public class HttpUtils
    {
        public static string GetPageSource(string address)
        {
            return new WebClient().DownloadString(address);
        }

        public static string GetPageSource(Uri address)
        {
            return new WebClient().DownloadString(address);
        }
    }
}
