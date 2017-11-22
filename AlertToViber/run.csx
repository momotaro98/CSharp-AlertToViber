#load "..\ViberApiLib\Api.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IQueryable<BotUser> tableBinding, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Validate that reqeust is from script on Splunk
    // [Caution] If we can use Webhook from Splunk, this validation should be removed.
    if (!req.Headers.Contains("X-Script-On-Splunk"))
    {
        log.Info("Got an incorrect request, which doesn't have X-Script-On-Splunk header");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();
    // Log Webhook data
    log.Info(data?.ToString());
    
    // Get result element from Splunk
    // string result = data?.result.ToString(); // Result from Webhook of Splunk
    string result = data?.ToString(); // Resulf from Script on Splunk

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Send Alert to Subscribers
    var viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE") ?? "";
    foreach (BotUser user in tableBinding.Where(u => u.PartitionKey == viberAlertBotName).ToList())
    {
        log.Info($"Send to {user.UserName}, UserId: {user.UserId}");
        var result_SendMessages = viber.SendMessages(userId: user.UserId, text: "Alert occurred\n" + result.ToString());
        log.Info(result_SendMessages);
    }

    return result == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a result on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "result: \n" + result.ToString());
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}