#load "..\ViberApiLib\Api.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get request headers
    if (!req.Headers.Contains("X-Viber-Content-Signature"))
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "event", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");
    name = viber.GetName();

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
