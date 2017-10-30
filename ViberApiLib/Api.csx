#load ".\Constants.csx"

#r "Newtonsoft.Json"

using Newtonsoft.Json;

public class Api
{
    private string _authToken;
    public string AuthToken
    { 
        get { return _authToken; }
    } 

    private string _botName;
    public string BotName
    {
        get { return _botName; }
    }

    private string _avatarPath;
    public string AvatarPath
    {
        get { return _avatarPath; }
    }

    public Api(string authToken, string botName, string avatarPath)
    {
        _authToken = authToken;
        _botName = botName;
        _avatarPath = avatarPath;
    }

    public string GetName()
    {
        return BotName;
    }

    public string SendMessages(string userId, string text, string trackingData = "")
    {
        var dictPayload = prepareSendMessagesPayload(message: text, receiver: userId, senderName: BotName, senderAvatar: AvatarPath, trackingData: trackingData);
        string paylaod = JsonConvert.SerializeObject(dictPayload);
        return PostRequest(Constants.SEND_MESSAGE, paylaod);
    }

    private Dictionary<string, object> prepareSendMessagesPayload(string message, string receiver, string senderName, string senderAvatar, string trackingData)
    {
        return new Dictionary<string, object>()
        {
            { "auth_token", AuthToken},
            { "receiver", receiver},
            { "min_api_version", 1},
            { "sender", new Dictionary<string, object>(){
                    { "name", senderName},
                    { "avatar", senderAvatar},
                }
            },
            { "tracking_data", trackingData},
            { "type", "text"},
            {"text", message},
        };
    }

    public string PostRequest(string endPoint, string payload)
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(Constants.VIBER_BOT_API_URL);
        StringContent content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");            
        // post data
        HttpResponseMessage response = client.PostAsync("/" + endPoint, content).Result;
        string resultContent = response.Content.ReadAsStringAsync().Result;
        return resultContent;
    }
}