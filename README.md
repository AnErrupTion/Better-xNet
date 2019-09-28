Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.3.7
**IMPORTANT :**
- Fixed all User-Agents.
- Switched to .NET Framework 4.5 to fix issue #59.

**Other :**
- Added all missing HTTP Status Codes (102, 103, 207, 208, 226, 308, 418 -> 429, 431, 451, 506 -> 508, 510, 511) and removed duplicate ones (including some of the Pull Request #58).
- Moved all events (DownloadProgressChangedEventArgs, MultiThreadingProgressEventArgs, etc..) to "xNet.Events".
- Removed the HttpHelper class, it was useless.
- Loading some User-Agents should now be faster due to removing useless parts.
- Added Brave and Chromium-based Edge User-Agent.
- Added back the *updated* Opera Mini User-Agent for Android only.
- Made the HttpUtils class static.
- Added the "TestWebsite" function in the HttpUtils class, made to test a website's availability with a 10s timeout by default.
- Getting a webpage's source with the "GetPageSource" function is now faster.

Is this library useful to you? If yes, then you should consider donating me (even a little) so I can get myself a cup of coffee!
PayPal : **erruption.selly@gmail.com** - Bitcoin : **1Q8dZDTDxzJ1YLbm4fYfEK8KvDq86LFucU**

Also check out my [YouTube channel](https://www.youtube.com/c/B3RAPSoftwares)!

Example :
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
