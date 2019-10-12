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
                req.Cookies = new CookieStorage();

                return req.Get(address).ToString();
            }

            return null;
        }

        public static bool TestWebsite(Uri address, string ua, int timeout = 10000)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.Cookies = new CookieStorage();
                req.ConnectTimeout = timeout;

                if (req.Get(address).IsOK) return true;
            }

            return false;
        }
    }
}
