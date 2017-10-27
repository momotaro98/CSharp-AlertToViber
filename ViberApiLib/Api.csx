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
}