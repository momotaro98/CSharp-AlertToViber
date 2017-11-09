#load ".\Constants.csx"
#load ".\UserProfile.csx"
#load ".\Message.csx"

#r "Newtonsoft.Json"

using Newtonsoft.Json;

public class RequstFactory
{
    private static readonly List<string> EventTypeList
        = new List<string>
    {
        Constants.EVENT_SEEN,
        Constants.EVENT_CONVERSATION_STARTED,
        Constants.EVENT_DELIVERED,
        Constants.EVENT_MESSAGE,
        Constants.EVENT_SUBSCRIBED,
        Constants.EVENT_UNSUBSCRIBED,
        Constants.EVENT_FAILED,
        Constants.EVENT_WEBHOOK
    };

    public Request Create(string jsonRequest)
    {
        var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonRequest);

        if (!values.ContainsKey("event"))
        {
            return null; // TODO : make good
        }

        var eventType = values["event"];

        if (!EventTypeList.Contains(eventType))
        {
            return null; // TODO : raise error
        }

        switch(eventType)
        {
            case Constants.EVENT_SUBSCRIBED:
                return new SubscribedRequest(values);
            case Constants.EVENT_UNSUBSCRIBED:
                return new UnsubscribedRequest(values);
            case Constants.EVENT_MESSAGE:
                return new MessageRequest(values);
            default: // TODO : increase types
                break;
        }
        return null; // TODO: make good
    }
}

public class Request
{
    private string _eventType;
    public string Event
    { 
        get { return _eventType; }
    }

    private string _timeStamp;
    public string TimeStamp
    {
        get { return _timeStamp; }
    }

    public Request(Dictionary<string, object> requestDict)
    {
        _eventType = requestDict["event"].ToString();
        _timeStamp = requestDict["timestamp"].ToString();
    }
}

public class SubscribedRequest : Request
{
    public UserProfile User { get; set; }

    public SubscribedRequest(Dictionary<string, object> requestDict) : base(requestDict)
    {
        string userStr = requestDict["user"].ToString();
        var userDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(userStr);
        User = new UserProfile(userDict);
    }
}

public class UnsubscribedRequest : Request
{
    private string _user_id;
    public string UserId
    {
        get { return _user_id; }
    }

    public UnsubscribedRequest(Dictionary<string, object> requestDict) : base(requestDict)
    {
        _user_id = requestDict["user_id"].ToString();
    }
}

public class MessageRequest : Request
{
    public UserProfile User { get; set; }
    public Message Message { get; set; }

    public MessageRequest(Dictionary<string, object> requestDict) : base(requestDict)
    {
        string userStr = requestDict["sender"].ToString();
        var userDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(userStr);
        User = new UserProfile(userDict);

        string messageStr = requestDict["message"].ToString();
        MessageFactory messageFactory = new MessageFactory();
        Message = messageFactory.Create(messageStr);
    }
}