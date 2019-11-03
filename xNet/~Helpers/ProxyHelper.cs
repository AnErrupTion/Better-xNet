using System;

namespace Better_xNet
{
    internal static class ProxyHelper
    {
        public static ProxyClient CreateProxyClient(ProxyType proxyType, string address, string username = null, string password = null)
        {
            string host = address.Split(':')[0];
            int port = int.Parse(address.Split(':')[1]);
            switch (proxyType)
            {
                case ProxyType.Http: return (port == 0) ? new HttpProxyClient(host) : new HttpProxyClient(host, port, username, password);
                case ProxyType.Socks4: return (port == 0) ? new Socks4ProxyClient(host) : new Socks4ProxyClient(host, port, username);
                case ProxyType.Socks5: return (port == 0) ? new Socks5ProxyClient(host) : new Socks5ProxyClient(host, port, username, password);
                default: throw new InvalidOperationException();
            }
        }
    }
}