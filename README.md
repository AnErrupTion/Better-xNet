Better-xNet - a fork of original xNet C# library, which includes all of these fixes & more :

Current version : 3.3.4
Changelog :
- Fixed the DLL not working.
- Removed Send() function.
- Added Edge User-Agent.
- Updated all current User-Agents.
- Switched to .NET Framework 4 again for better compatibility.
- Made a minimal change in the CookieStorage class.
- Added a new class named HttpUtils, including GetPageSource() function.
- Fixed Issue #59 (view issue on original xNet repository).

Example:
<pre>
using (var request = new HttpRequest("https://google.com/"))
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
		
    // Sending a post request.
    var somerequest = request.Post("..");
}
</pre>
