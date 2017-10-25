using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;

    // Use ViberApi
    ViberApi viber = new ViberApi("authAuth", "VIVIBER_TARO", "https://yamada.com.com");
    name = viber.GetName();

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}


public class ViberApi
{
    private string _authToken;
    public string AuthToken
    { 
        get { return _authToken; }
    } 

    private string _botName;
    public string BotName
    {
        get { return _botName; }
    }

    private string _avatarPath;
    public string AvatarPath
    {
        get { return _avatarPath; }
    }

    public ViberApi(string authToken, string botName, string avatarPath)
    {
        _authToken = authToken;
        _botName = botName;
        _avatarPath = avatarPath;
    }

    public string GetName()
    {
        return BotName;
    }
}

public class Request
{
}