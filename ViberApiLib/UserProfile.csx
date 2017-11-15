public class UserProfile
{
    private string _name;
    public string Name
    {
        get { return _name; }
    }
    
    private string _avatar;
    public string Avatar
    {
        get { return _avatar; }
    }

    private string _id;
    public string Id
    {
        get { return _id; }
    }

    private string _country;
    public string Country
    {
        get { return _country; }
    }

    private string _language;
    public string Language
    {
        get { return _language; }
    }

    private string _api_version;
    public string ApiVersion
    {
        get { return _api_version; }
    }

    public UserProfile(Dictionary<string, object> dict)
    {
        _id = dict["id"].ToString();
        _api_version = dict["api_version"].ToString();

        // There is a case that the following keys are not contained in payload of Viber API.
        _name = dict.ContainsKey("name") ? dict["name"].ToString() : "Nameless User";
        _avatar = dict.ContainsKey("avatar") ? dict["avatar"].ToString() : string.Empty;
        _country = dict.ContainsKey("country") ? dict["country"].ToString() : string.Empty;
        _language = dict.ContainsKey("language") ? dict["language"].ToString() : string.Empty;
    }
}