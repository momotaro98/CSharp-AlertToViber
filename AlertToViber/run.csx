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
    if (data?.ToString() == null) return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    string dataStr = data?.ToString();
    // Log request data
    log.Info("Request body");
    log.Info(dataStr);

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Get Splunk data
    // http://docs.splunk.com/Documentation/Splunk/6.6.3/Alert/Configuringscriptedalerts#Access_arguments_to_scripts_that_are_run_as_an_alert_action
    var queryString = data?.SPLUNK_ARG_3.ToString();
    log.Info($"fully_qualified_query_string: {queryString}");
    var nameOfReport = data?.SPLUNK_ARG_4.ToString();
    log.Info($"name_of_report: {nameOfReport}");

    // Create alert message to Viber
    var messageAlertName = string.Format("[Alert Name] {0}", nameOfReport);
    var messageQueryString = string.Format("[Splunk Query] {0}", queryString);
    var alertMessageToViber = "Alert occurred\n" + "\n" + messageAlertName + "\n" + messageQueryString;

    // Send Alert to Subscribers
    var viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE") ?? "";
    foreach (BotUser user in tableBinding.Where(u => u.PartitionKey == viberAlertBotName).ToList())
    {
        log.Info($"Send to {user.UserName}, UserId: {user.UserId}");
        var result_SendMessages = viber.SendMessages(userId: user.UserId, text: alertMessageToViber);
        log.Info(result_SendMessages);
    }

    return req.CreateResponse(HttpStatusCode.OK, "Splunk post data: \n" + dataStr);
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}