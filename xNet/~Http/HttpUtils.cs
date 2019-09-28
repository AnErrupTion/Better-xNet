using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Better_xNet
{
    public static class HttpUtils
    {
        public static string GetPageSource(string address, string ua)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.Cookies = new CookieStorage();

                string respo = req.Get(address).ToString();
                return respo;
            }

            return null;
        }

        public static string GetPageSource(Uri address, string ua)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.Cookies = new CookieStorage();

                string respo = req.Get(address).ToString();
                return respo;
            }

            return null;
        }

        public static bool TestWebsite(string address, string ua, int timeout = 10000)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.Cookies = new CookieStorage();
                req.ConnectTimeout = timeout;

                bool respo = req.Get(address).IsOK;
                if (respo) return true;
            }

            return false;
        }

        public static bool TestWebsite(Uri address, string ua, int timeout = 10000)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = ua;
                req.KeepAlive = true;
                req.Cookies = new CookieStorage();
                req.ConnectTimeout = timeout;

                bool respo = req.Get(address).IsOK;
                if (respo) return true;
            }

            return false;
        }
    }
}
