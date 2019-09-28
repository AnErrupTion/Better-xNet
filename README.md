Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.3.6
- Switched the assembly's title and description.
- Renamed the "Html.cs" class to "TextHelper.cs" and moved it to "xNet.Text.TextHelper".
- Added the "SecureRandoms.cs" class that contains secure cryptographical randoms. It's in "xNet.Text.Cryptography.SecureRandoms".
- Added back the MD5 functions from the 3.1.4 DLL (thanks to @nguyenvu1981 for giving me the link of the 3.1.4 version of xNet). The class is in "xNet.Text.Cryptography.MD5Hashes".
- Added back the MultiThreading with his other classes "AsyncEvent" and 2 others from the 3.1.4 DLL. They're in "xNet.Threading.MultiThreading", "xNet.Threading.AsyncEvent", etc..

Example:
<pre>
using (var request = new HttpRequest("https://google.com/"))
{
    request.UserAgent = Http.EdgeUserAgent();
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
