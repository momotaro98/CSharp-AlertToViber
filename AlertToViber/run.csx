#load "..\ViberApiLib\Api.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using System.Text.RegularExpressions;
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

    string subject = null;
    
    // string pattern = @"Subject.";  // --- OK
    // string pattern = @"Subject.*"; // --- OK
    // string pattern = @"Subject.*$"; // --- NG
    string pattern = @"(?<=Subject:\s).*"; // To get alert email subject
    
    log.Info("Logging HttpContents from SendGrid");
    foreach (var cntnt in provider.Contents)
    {
        /* provider.Contents from SendGrid contains HttpContent that has "Subject: subject..." */

        // Get Content from SendGrid
        var item = await cntnt.ReadAsStringAsync();
        log.Info(item.ToString());
        
        // Get alert email subject
        Match matchedObject = Regex.Match(item.ToString(), pattern);
        string matchedString = matchedObject?.Value.ToString();
        if (matchedString != null && matchedString != string.Empty)
        {
            subject = matchedString;
        }
    }

    log.Info($"Alert email subject obtained : {subject?.ToString()}");

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Create alert message to Viber
    var messageSubject = string.Format("[Email Subject] {0}", subject?.ToString());
    var alertMessageToViber = "Alert occurred\n" + "\n" + messageSubject;
    log.Info($"Alert Message to Viber: {alertMessageToViber.ToString()}");

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