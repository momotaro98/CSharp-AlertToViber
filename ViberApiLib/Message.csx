#r "Newtonsoft.Json"

using Newtonsoft.Json;

public class MessageFactory
{
    private static readonly List<string> MessageTypeList
        = new List<string>
    {
        Constants.RICH_MEDIA,
        Constants.STICKER,
        Constants.URL,
        Constants.LOCATION,
        Constants.CONTACT,
        Constants.FILE,
        Constants.TEXT,
        Constants.PICTURE,
        Constants.VIDEO,
        Constants.KEYBOARD
    };

    public Message Create(string jsonString)
    {
        var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

        if (!values.ContainsKey("type"))
        {
            throw new KeyNotFoundException("Necessary key of Viber message, \"type\" is not in the request payload.");
        }

        var type = values["type"];

        if (!MessageTypeList.Contains(type))
        {
            throw new Exception("\"type\" key's value of the Viber message, " + type.ToString() + " is unknown.");
        }

        switch(type)
        {
            case Constants.TEXT:
                return new TextMessage(values);
            default: // TODO : Add more message types of Viber
                break;
        }
        return null; // never reach
    }
}

public class Message
{
    private string _type;
    public string Type
    {
        get { return _type; }
    }

    private string _trackingData;
    public string TrackingData
    {
        get { return _trackingData; }
    }

    public Message(Dictionary<string, object> dict)
    {
        _type = dict["type"].ToString();
        _trackingData = dict["tracking_data"].ToString();
    }
}

public class TextMessage : Message
{
    private string _text;
    public string Text
    {
        get { return _text; }
    }

    public TextMessage(Dictionary<string, object> dict) : base(dict)
    {
        _text = dict["text"].ToString();
    }
}

// TODO : Add more message types of Viber