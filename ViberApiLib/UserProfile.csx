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
        _name = dict["name"].ToString();
        _avatar = dict["avatar"].ToString();
        _id = dict["id"].ToString();
        _country = dict["country"].ToString();
        _language = dict["language"].ToString();
        _api_version = dict["api_version"].ToString();
    }
}