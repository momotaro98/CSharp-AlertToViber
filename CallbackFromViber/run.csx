#load "..\ViberApiLib\Api.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<BotUser> tableBinding, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get request headers
    if (!req.Headers.Contains("X-Viber-Content-Signature"))
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");
    
    // Parse request
    var viberRequest = viber.ParseRequest(data.ToString());

    if (viberRequest is SubscribedRequest)
    {
        log.Info("Subscribed Request");
        log.Info(viberRequest.TimeStamp);
        var userId = viberRequest.User.Id;
        var userName = viberRequest.User.Name;

        log.Info($"Adding BotUser Entity , UserName: {userName}, UserId: {userId}");

        // Get BotUser Table's PartitionKey
        string viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE");
        
        // Generate random value for adding row key of BotUser table
        Random random = new Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string randomStr = new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        // Register User
        tableBinding.Add(
            new BotUser() { 
                PartitionKey = viberAlertBotName, 
                RowKey = randomStr,
                UserId = userId,
                UserName = userName }
            );
    }

    return viberRequest == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello Viber");
}

public class BotUser
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}