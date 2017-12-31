#load "..\ViberApiLib\Api.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IQueryable<BotUser> tableBinding, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Validate if the request is from SendGrid
    if (!req.Headers.Contains("X-WAWS-Unencoded-URL"))
    {
        log.Info("Got an incorrect request, which doesn't have X-WAWS-Unencoded-URL header");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");
    }

    // Get request body
    var provider = new MultipartMemoryStreamProvider();
    await req.Content.ReadAsMultipartAsync(provider);
    if (provider?.ToString() == null) return req.CreateResponse(HttpStatusCode.BadRequest, "Incorrect request");

    string subject = null;
    
    // string pattern = @"Subject.";  // --- OK
    // string pattern = @"Subject.*"; // --- OK
    // string pattern = @"Subject.*$"; // --- NG
    string pattern = @"(?<=Subject:\s).*"; // To get alert email subject
    
    log.Info("Logging HttpContents from SendGrid");
    foreach (var cntnt in provider.Contents)
    {
        /* provider.Contents from SendGrid contains HttpContent that has "Subject: subject..." */

        // Get Content from SendGrid
        var item = await cntnt.ReadAsStringAsync();
        log.Info(item.ToString());
        
        // Get alert email subject
        Match matchedObject = Regex.Match(item.ToString(), pattern);
        string matchedString = matchedObject?.Value.ToString();

        // Check if the matched string is empty
        if (matchedString != null && matchedString != string.Empty)
        {
            // Parse and Decode and Join if the string type is UTF-8
            if (matchedString.StartsWith("=?"))
            {
                log.Info("String encoding of subject of this incoming email might be not ascii");
                matchedString = ParseAndDecodeAndJoin(matchedString);
            }

            // Set subject of email
            if (string.IsNullOrEmpty(subject))
            {
                subject = matchedString;
            }
            else
            {
                log.Info($"current extracted subject: {subject?.ToString()}");
                
                if (matchedString.StartsWith("Re:") || matchedString.StartsWith("RE:"))
                {
                    subject = matchedString;
                }
            }
        }
    }

    log.Info($"Alert email subject obtained : {subject?.ToString()}");

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);

    // Create alert message to Viber
    var messageSubject = string.Format("[Email Subject] {0}", subject?.ToString());
    var alertMessageToViber = "Alert occurred\n" + "\n" + messageSubject;
    log.Info($"Alert Message to Viber: {alertMessageToViber.ToString()}");

    // Send Alert to Subscribers
    var viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE") ?? "";
    foreach (BotUser user in tableBinding.Where(u => u.PartitionKey == viberAlertBotName).ToList())
    {
        log.Info($"Send to {user.UserName}, UserId: {user.UserId}");
        var result_SendMessages = viber.SendMessages(userId: user.UserId, text: alertMessageToViber);
        log.Info(result_SendMessages);
    }

    return req.CreateResponse(HttpStatusCode.OK);
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}

public static string ParseAndDecodeAndJoin(string str)
{
    /*
        [CASE 1]
        input: "=?utf-8?B?Lm1zLmpwLmxvY2FsIOOBp+aknOWHuuOBleOCjOOBvuOBl+OBnw==?="
        return: "高リスクの侵入が WRSLOMS101Z"

        [CASE 2]
        input: "=?utf-8?B?6auY44Oq44K544Kv44Gu5L615YWl44GMIFdSU0xPTVMxMDFa?= =?utf-8?B?Lm1zLmpwLmxvY2FsIOOBp+aknOWHuuOBleOCjOOBvuOBl+OBnw==?="
        return: "高リスクの侵入が WRSLOMS101Z.ms.jp.local で検出されました" 

        [CASE 3]
        input: "=?iso-2022-jp?B?GyRCJEskWyRzJDQkQCQxGyhC?="
        return: "にほんごだけ"
    */

    // Return arg string itself if the encoding is not targeted
    if (!str.StartsWith("=?utf-8?B?") && !str.StartsWith("=?UTF-8?B?")
        && !str.StartsWith("=?iso-2022-jp?B?") && !str.StartsWith("=?ISO-2022-JP?B?"))
    {
        return str;
    }

    // Parse with "?"
    Char delimiter = '?';
    String[] substrings = str.Split(delimiter);
    string joinedText = string.Empty;
    int n = substrings.Length / 4;
    for (int i = 0; i < n; i++)
    {
        // Decode Base64
        var bytes = Convert.FromBase64String(substrings[4*i + 3]);

        // String to join
        string text = string.Empty;
        // Decode according to encoding
        switch (substrings[4*i + 1])
        {
        case "UTF-8":
        case "utf-8":
            text = Encoding.UTF8.GetString(bytes);
            break;
        case "ISO-2022-JP":
        case "iso-2022-jp":
            text = Encoding.GetEncoding("iso-2022-jp").GetString(bytes);
            break;
        default:
            text = str;
            break;
        }

        // Join
        joinedText += text;
    }
    return joinedText;
}