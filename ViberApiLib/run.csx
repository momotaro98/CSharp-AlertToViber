#load ".\Api.csx"
#load ".\Request.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();
    log.Info(data?.GetType().ToString());

    // Test ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("TEST_VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, TEST_VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("TEST_BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("TEST_BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Test SendMessages
    var result = viber.SendMessages(userId: "nie7s9b4vcXqc/yfbyJyGw==", text: "Test Api.SendMessages method"); // TODO: Replace
    log.Info($"SendMessages method result: {result}");

    // Test SetWebhook
    var event_type = viber.SetWebhook(System.Environment.GetEnvironmentVariable("API_CALLBACK_FROM_VIBER"));
    log.Info($"SetWebhook method result: {event_type}");

    // Test VerifySignature TODO : Work Well
    /*
    IEnumerable<string> headerValues = req.Headers.GetValues("X-Viber-Content-Signature");
    var signature = headerValues.FirstOrDefault();
    bool ok = viber.VerifySignature(data.ToString(), signature.ToString());
    if (!ok) log.Info("Error! Not Verified");
    */

    // Test ParseRequest
    try
    {
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
        else if (viberRequest is SeenRequest)
        {
            /* TestCase of SeenRequest callback data from public site
            {
                "event":"seen",
                "timestamp":1457764197627,
                "message_token":4912661846655238145,
                "user_id":"01234567890A="
            }
            */
            log.Info("Seen Request");
            log.Info(viberRequest.Event);
            log.Info(viberRequest.TimeStamp);
            log.Info(viberRequest.UserId);
        }
        else if (viberRequest is ConversationStartedRequest)
        {
            /* TestCase of ConversationStartedRequest callback data from public site
            {
                "event":"conversation_started",
                "timestamp":1457764197627,
                "message_token":4912661846655238145,
                "type":"open",
                "context":"context information",
                "user":{
                    "id":"01234567890A=",
                    "name":"John McClane",
                    "avatar":"http://avatar.example.com",
                    "country":"UK",
                    "language":"en",
                    "api_version":1
                },
                "subscribed":false
            }
            */
            log.Info("ConversationStarted Request");

            log.Info(viberRequest.Event);
            log.Info(viberRequest.TimeStamp);

            log.Info(viberRequest.User.Id);
            log.Info(viberRequest.User.Name);
            log.Info(viberRequest.User.Avatar);
            log.Info(viberRequest.User.Country);
            log.Info(viberRequest.User.Language);
            log.Info(viberRequest.User.ApiVersion);
        }
        else if (viberRequest is DeliveredRequest)
        {
            /* TestCase of DeliveredRequest callback data from public site
            {
                "event":"delivered",
                "timestamp":1457764197627,
                "message_token":4912661846655238145,
                "user_id":"01234567890A="
            }
            */
            log.Info("Delivered Request");
            log.Info(viberRequest.Event);
            log.Info(viberRequest.TimeStamp);
            log.Info(viberRequest.UserId);
        }
        else if (viberRequest is FailedRequest)
        {
            /* TestCase of FailedRequest callback data from public site
            {
                "event":"failed",
                "timestamp":1457764197627,
                "message_token":4912661846655238145,
                "user_id":"01234567890A=",
                "desc":"failure description"
            }
            */
            log.Info("Failed Request");
            log.Info(viberRequest.Event);
            log.Info(viberRequest.TimeStamp);
            log.Info(viberRequest.UserId);
        }
        else if (viberRequest is WebhookRequest)
        {
            /* TestCase of WebhookRequest callback data from public site
            {
                "event":"webhook",
                "timestamp":1457764197627,
                "message_token":"241256543215"
            }
            */
            log.Info("Webhook Request");
            log.Info(viberRequest.Event);
            log.Info(viberRequest.TimeStamp);
        }
    }
    catch (KeyNotFoundException e)
    {
        log.Info(e.ToString());
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a correct payload in the request body");
    }
    catch (Exception e)
    {
        log.Info(e.ToString());
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a correct payload in the request body");
    }
    
    return req.CreateResponse(HttpStatusCode.OK, "Hello Viber API");
}
