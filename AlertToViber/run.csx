#load "..\ViberApiLib\Api.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IQueryable<BotUser> tableBinding, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();
    // Log Webhook data
    log.Info(data?.ToString());
    // Get result element from Splunk
    string result = data?.result.ToString();

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");

    // Send Alert to Subscribers
    string viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE");
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