Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.3.8
# Changelog :
- Added the RandomUserAgent() function.
- Dropped support for string address, must use Uri (HttpRequest, HttpUtils).
- Made the Raw() function private.
- Dropped support for Socks4A proxies.

# Donation :
Is this library useful to you? If yes, then you should consider donating me (even a little) so I can get myself a cup of coffee!
PayPal : **erruption.selly@gmail.com** - Bitcoin : **1Q8dZDTDxzJ1YLbm4fYfEK8KvDq86LFucU**

Also check out my [YouTube channel](https://www.youtube.com/c/B3RAPSoftwares)!

# Example :
<pre>
using (var req = new HttpRequest())
{
   req.UserAgent = Http.EdgeUserAgent();
   req.Proxy = Socks4ProxyClient.Parse("191.7.50.119:4145");
   req.KeepAlive = true;
   req.Cookies = new CookieStorage();
   // Note that req.IgnoreProtocolErrors is already true by default.

   req.Get("https://httpbin.org/get").ToString(); // Sending a GET request without any parameters
   
   req.AddParam("email", "erruption.selly@gmail.com");
   req.AddParam("password", "c0ns1d3rD0nat1ngM3!");
   
   req.Post("https://httpbin.org/post").ToString(); // Sending a POST request with parameters "email" and "password".
}
</pre>
