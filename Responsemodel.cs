
public class Rootobject
{
    public bool success { get; set; }
    public string access_token { get; set; }
    public Request Request { get; set; }
    public int expires_in { get; set; }
    public Response Response { get; set; }
    public string refresh_token { get; set; }
}

public class Request
{
    public string abs_path { get; set; }
    public string absoluteUri { get; set; }
    public string HTTPVersion { get; set; }
    public Headers headers { get; set; }
    public string host { get; set; }
    public string messagebody { get; set; }
    public string Method { get; set; }
}

public class Headers
{
    public string Contentlength { get; set; }
    public string contenttype { get; set; }
    public string useragent { get; set; }
}

public class Response
{
    public string ReasonPhrase { get; set; }
    public string HTTPVersion { get; set; }
    public Headers1 headers { get; set; }
    public string messagebody { get; set; }
    public int StatusCode { get; set; }
    public bool isbinary { get; set; }
}

public class Headers1
{
    public string Contentlength { get; set; }
    public string Xxssprotection { get; set; }
    public string Xcontenttypeoptions { get; set; }
    public string Transferencoding { get; set; }
    public string Expires { get; set; }
    public string Vary { get; set; }
    public string Server { get; set; }
    public string contentencoding { get; set; }
    public string Pragma { get; set; }
    public string Cachecontrol { get; set; }
    public string Date { get; set; }
    public string Xframeoptions { get; set; }
    public string Altsvc { get; set; }
    public string Contenttype { get; set; }
}
