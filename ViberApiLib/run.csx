#load ".\Api.csx"
#load ".\Request.csx"

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
    log.Info(viberRequest.Event);
    log.Info(viberRequest.TimeStamp);

    if (viberRequest is SubscribedRequest)
    {
        /* TestCase of Subscribed callback data from public site
        {
            "event":"subscribed",
            "timestamp":1457764197627,
            "user":{
                "id":"01234567890A=",
                "name":"John McClane",
                "avatar":"http://avatar.example.com",
                "country":"UK",
                "language":"en",
                "api_version":1
            },
            "message_token":4912661846655238145
        }
        */
        log.Info("Subscribed Request");
        log.Info(viberRequest.Event);
        log.Info(viberRequest.TimeStamp);
        log.Info(viberRequest.User.Id);
        log.Info(viberRequest.User.Name);
        log.Info(viberRequest.User.Avatar);
        log.Info(viberRequest.User.Country);
        log.Info(viberRequest.User.Language);
        log.Info(viberRequest.User.ApiVersion);
    }
    else if (viberRequest is UnsubscribedRequest)
    {
        /* TestCase of Unsubscribed callback data from public site
        {
            "event":"unsubscribed",
            "timestamp":1457764197627,
            "user_id":"01234567890A=",
            "message_token":4912661846655238145
        }
        */
        log.Info("Unsubscribed Request");
        log.Info(viberRequest.Event);
        log.Info(viberRequest.TimeStamp);
        log.Info(viberRequest.UserId);
    }
    else if (viberRequest is MessageRequest)
    {
        /* TestCase of MessageRequest callback data from public site
        {
            "event":"message",
            "timestamp":1457764197627,
            "message_token":4912661846655238145,
            "sender":{
                "id":"01234567890A=",
                "name":"John McClane",
                "avatar":"http://avatar.example.com",
                "country":"UK",
                "language":"en",
                "api_version":1
            },
            "message":{
                "type":"text",
                "text":"a message to the service",
                "media":"http://example.com",
                "location":{
                    "lat":50.76891,
                    "lon":6.11499
                },
                "tracking_data":"tracking data"
            }
        }
        */
        log.Info("Message Request");

        log.Info(viberRequest.Event);
        log.Info(viberRequest.TimeStamp);

        log.Info(viberRequest.User.Id);
        log.Info(viberRequest.User.Name);
        log.Info(viberRequest.User.Avatar);
        log.Info(viberRequest.User.Country);
        log.Info(viberRequest.User.Language);
        log.Info(viberRequest.User.ApiVersion);

        log.Info(viberRequest.Message.Type);
        log.Info(viberRequest.Message.TrackingData);
        log.Info(viberRequest.Message.Text);
    }
    
    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}
