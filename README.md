Better-xNet - a fork of the original xNet C# library, which includes all of these fixes & more :

Current version : 3.3.5
- Renamed a folder in the project (just to match the GitHub code).
- Added Windows 8.1 version for the User-Agents.
- Translated all exceptions messages to English.

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
