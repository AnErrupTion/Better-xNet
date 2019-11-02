using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Better_xNet
{
    public static class HttpUtils
    {

        public static string GetPageSource(Uri address, string ua)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                return req.Send(HttpMethod.GET, address).ToString();
            }
            return string.Empty;
        }

        public static bool TestWebsite(Uri address, string ua, int timeout = 10000)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.ConnectTimeout = timeout;
                if (req.Send(HttpMethod.GET, address).IsOK) return true;
            }
            return false;
        }
    }
}
