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

    // Set name to query string or body data
    name = name ?? data?.name;

    // Test ViberApi
    Api viber = new Api("abcdefxxxxx", "VIVIBER_TARO", ""); // TODO : Replace
    name = viber.GetName();
    if (name != "VIVIBER_TARO") log.Error("Test Api.GetName failed"); // TODO: Make test method better

    var result = viber.SendMessages(userId: "nie7s9b4vcXqc/yfbyJyGw==", text: "TEST VIBER!!!"); // TODO: Replace
    log.Info(result.ToString());

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
