#load "..\ViberApiLib\Api.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<BotUser> tableBinding, ICollector<string> outputQueueItem, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get request headers
    if (!req.Headers.Contains("X-Viber-Content-Signature"))
    {
        log.Info("Got an incorrect request, which doesn't have X-Viber-Content-Signature header");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Log Viber callback post body
    log.Info("Viber callback post body: ");
    log.Info(data?.ToString());

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);
        
    // Parse request
    var viberRequest = viber.ParseRequest(data.ToString());

    // Log request function got
    log.Info($"Viber callback event type: {viberRequest.Event}");

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
                    
        // Send welcome message to new subscriber
        viber.SendMessages(userId: userId, text: "Hi, " + userName + "\nWelcome!");
    }
    else if (viberRequest is UnsubscribedRequest)
    {
        log.Info("Unsubscribed Request");
        log.Info(viberRequest.TimeStamp);
        var userId = viberRequest.UserId;
        log.Info($"Deleting BotUser Entity , UserId: {userId}");

        // Add UserId to queue to kick DeleteBotUserEntity app
        outputQueueItem.Add(userId);
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