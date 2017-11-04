#load "..\ViberApiLib\Api.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "event", true) == 0)
        .Value;

    // Get request headers
    IEnumerable<string> headerValues = req.Headers.GetValues("X-Viber-Content-Signature");
    var signature = headerValues.FirstOrDefault();

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");
    name = viber.GetName();

    // Verify TODO : Work well
    /*
    bool ok = viber.VerifySignature(data.ToString(), signature);
    if (!ok) log.Info("Error! Not Verified");
    */

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
