#load "..\ViberApiLib\Api.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IQueryable<BotUser> tableBinding, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Validate if the request is from SendGrid
    if (!req.Headers.Contains("X-WAWS-Unencoded-URL"))
    {
        log.Info("Got an incorrect request, which doesn't have X-WAWS-Unencoded-URL header");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // Get request body
    var provider = new MultipartMemoryStreamProvider();
    await req.Content.ReadAsMultipartAsync(provider);
    if (provider?.ToString() == null) return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");

    // Parse Email from SendGrid
    log.Info("Parsing Email from SendGrid");
    var Subject = provider.Contents[SendGridConstants.SubjectIndex].ReadAsStringAsync().Result;
    log.Info($"Email Subject: {Subject.ToString()}");
    var Body = provider.Contents[SendGridConstants.BodyIndex].ReadAsStringAsync().Result;
    log.Info($"Email Body: {Body.ToString()}");

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Create alert message to Viber
    var messageSubject = string.Format("[Email Subject] {0}", Subject);
    var messageBody = string.Format("[Email Body] {0}", Body);
    var alertMessageToViber = "Alert occurred\n" + "\n" + messageSubject + "\n" + messageBody;

    // Send Alert to Subscribers
    var viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE") ?? "";
    foreach (BotUser user in tableBinding.Where(u => u.PartitionKey == viberAlertBotName).ToList())
    {
        log.Info($"Send to {user.UserName}, UserId: {user.UserId}");
        var result_SendMessages = viber.SendMessages(userId: user.UserId, text: alertMessageToViber);
        log.Info(result_SendMessages);
    }

    return req.CreateResponse(HttpStatusCode.OK);
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}

static class SendGridConstants
{
    public static readonly int SubjectIndex = 9;
    public static readonly int BodyIndex = 5;
}