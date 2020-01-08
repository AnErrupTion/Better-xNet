# NOTE : This project is currently dead. If you want a more stable HTTP library, check out my other project named ReqDotNet.

# Thanks everyone!
Thanks to everyone for starring my xNet fork, it was my biggest C# project back in the days. Now, I'm making other projects that interests me even more.
You can fork my project if you want to keep it alive! :D

Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.4.3
# Changelog :
- Optimized the HttpUtils's GetPageSource() and TestWebsite() functions.
- Made some minor changes in the CookieStorage class and HttpRequest.Request() function (private).
- Now using an improved version of the MemoryStream class called MemoryTributary and edited it to match xNet's needs. This version is used in the HttpResponse.ToString() method.
- Also made some minor changes in the ProxyHelper and ProxyClient.

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
   
   // Updated use of the parameters in a request :
   // -- Normal :
   req.Send(HttpMethod.POST, new Uri("https://httpbin.org/post"), new BytesContent(Encoding.UTF8.GetBytes("email=ichicharka@gmail.com&password=c0ns1d3rD0nat1ngM3!")).ToString();
   
   // -- JSON :
   req.Send(HttpMethod.POST, new Uri("https://httpbin.org/post"), new BytesContent(Encoding.UTF8.GetBytes("{\"email\":\"ichicharka@gmail.com\",\"password\":\"c0ns1d3rD0nat1ngM3!\"}"))).ToString()
}
</pre>
