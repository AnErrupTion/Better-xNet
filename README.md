Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.4.2.1
# Changelog :
- **Fixing a critical bug with the TLS version 1.3 support, the library now uses TLS version 1.2 and is reverted back to .NET Framework 4.5.**

# Donation :
Is this library useful to you? If yes, then you should consider donating me (even a little) so I can get myself a cup of coffee!
PayPal : **erruption.selly@gmail.com** - Bitcoin : **1Q8dZDTDxzJ1YLbm4fYfEK8KvDq86LFucU**

Also check out my [YouTube channel](https://www.youtube.com/c/B3RAPSoftwares)!

# Example (updated for the version 3.4.2) :
<pre>
using (HttpRequest req = new HttpRequest())
{
   req.UserAgent = Http.PaleMoonUserAgent(); // Basically this is what browser you will choose to make your request
   req.Proxy = Socks4ProxyClient.Parse("177.10.144.22:1080"); // Not needed here but it's an example
   req.KeepAlive = true; // Sometimes useful
   req.Cookies = new CookieStorage(); // Same as the proxy
   // Note that req.CheckBadStatusCode is already false by default.

   req.Send(HttpMethod.GET, new Uri("https://httpbin.org/get")).ToString(); // Sending a GET request without any parameters
   // Also note that the example above doesn't contain any HttpContent because we don't need any.
   
   // DEPRECATED USE OF THE PARAMETERS IN A REQUEST, NEEDS TO BE UPDATED :
   req.AddParam("email", "erruption.selly@gmail.com");
   req.AddParam("password", "c0ns1d3rD0nat1ngM3!");

   req.Send(HttpMethod.POST, new Uri("https://httpbin.org/post")).ToString(); // Sending a POST request with parameters "email" and "password".
}
</pre>
