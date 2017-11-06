#load ".\Api.csx"

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
    log.Info(data?.GetType().ToString());

    // Set name to query string or body data
    name = name ?? data?.name;

    // Test ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("TEST_VIBER_AUTH_TOKEN"), "momotaroBot", "");
    name = viber.GetName();  // TODO : Remove GetName method
    if (name != "momotaroBot") log.Error("Test Api.GetName failed"); // TODO: Make better

    // Test SendMessages
    var result = viber.SendMessages(userId: "nie7s9b4vcXqc/yfbyJyGw==", text: "Test Api.SendMessages method"); // TODO: Replace
    log.Info(result);

    // Test SetWebhook
    var event_type = viber.SetWebhook(System.Environment.GetEnvironmentVariable("API_CALLBACK_FROM_VIBER"));
    log.Info(event_type);

    // Test VerifySignature TODO : Work Well
    /*
    IEnumerable<string> headerValues = req.Headers.GetValues("X-Viber-Content-Signature");
    var signature = headerValues.FirstOrDefault();
    bool ok = viber.VerifySignature(data.ToString(), signature.ToString());
    if (!ok) log.Info("Error! Not Verified");
    */

    // Test ParseRequest
    var viberRequest = viber.ParseRequest(data.ToString());
    log.Info(viberRequest.EventType);
    log.Info(viberRequest.TimeStamp);

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
