#load ".\Constants.csx"

#r "Newtonsoft.Json"

using System.Security.Cryptography;
using System.Text;
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

    public string SetWebhook(string url, List<string> event_types = null)
    {
        var dictPayload = new Dictionary<string, object>()
        {
            { "auth_token", AuthToken},
            { "url", url},
        };
        if (event_types != null && event_types.Count > 0)
        {
            dictPayload.Add("event_types", event_types);
        }
        string paylaod = JsonConvert.SerializeObject(dictPayload);
        var result = PostRequest(Constants.SET_WEBHOOK, paylaod);
        var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        if (values["status"].ToString() != "0")
        {
            return string.Format("Failed with status: {0}, massage: {1}", values["status"], values["status_message"]);
        }
        return values["event_types"].ToString();
    }

    public bool VerifySignature(string requestData, string signature)
    {
        return signature == calculateMessageSignature(requestData);
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
        StringContent content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
        // post data
        HttpResponseMessage response = client.PostAsync(Constants.VIBER_BOT_API_URL + "/" + endPoint, content).Result;
        // TODO : Handle request errors
        return response.Content.ReadAsStringAsync().Result;
    }

    private string calculateMessageSignature(string message)
    {
        byte[] keyByte = new ASCIIEncoding().GetBytes(AuthToken);
        byte[] messageBytes = new ASCIIEncoding().GetBytes(message);

        byte[] hashmessage = new HMACSHA256(keyByte).ComputeHash(messageBytes);
        return string.Concat(Array.ConvertAll(hashmessage, x => x.ToString("x2")));
    }
}