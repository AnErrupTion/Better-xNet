Better-xNet - a fork of original xNet C# library, which includes all of these fixes & more :
- Removed Chain proxy support
- Fixed Issue #54 (view issue on original xNet repository).
- Removed Opera Mini user agent.
- IgnoreProtocolErrors is now true by default.
- Changed CookieDictionary to CookieStorage and made some minimal changes.
- Added Send() function, better than using Post(), Get() etc..
- Using .NET Framework 4.8 for better compatibility.
- Fixed Issue #34 (view issue on original xNet repository).

Example:
<pre>
using (var request = new HttpRequest("http://site.com/"))
{
    request.UserAgent = Http.ChromeUserAgent();
	request.Proxy = Socks5ProxyClient.Parse("127.0.0.1:1080");

    request
        // Parameters URL-address.
        .AddUrlParam("data1", "value1")
        .AddUrlParam("data2", "value2")

        // Parameters 'x-www-form-urlencoded'.
        .AddParam("data1", "value1")
        .AddParam("data2", "value2")
        .AddParam("data2", "value2")

        // Multipart data.
        .AddField("data1", "value1")
        .AddFile("game_code", @"C:\orion.zip")

        // HTTP-header.
        .AddHeader("X-Apocalypse", "21.12.12");
		
    // Sending a post request through the original method
    var somerequest = request.Post("..");
    
    // Sending a post request through new method
    var NEWsomerequest = request.Send(HttpMethod.POST, "..", new HttpContent(..));
}
</pre>
