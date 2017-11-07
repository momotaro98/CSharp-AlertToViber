#load "..\ViberApiLib\Api.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
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

    var result_SendMessages = viber.SendMessages(userId: "nie7s9b4vcXqc/yfbyJyGw==", text: "Alert occurred\n" + result.ToString());
    log.Info(result_SendMessages);

    return result == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a result on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "result: \n" + result.ToString());
}