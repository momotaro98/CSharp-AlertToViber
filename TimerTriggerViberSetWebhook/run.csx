#load "..\ViberApiLib\Api.csx"

using System;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    // Use ViberApi
    var authToken = System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN");
    if (authToken == null) log.Info("Environment Variable, VIBER_AUTH_TOKEN, is not set.");
    var botName = System.Environment.GetEnvironmentVariable("BOT_NAME") ?? "";
    var botAvatar = System.Environment.GetEnvironmentVariable("BOT_AVATAR_URI") ?? "";
    Api viber = new Api(authToken, botName, botAvatar);
    
    log.Info($"Setting Viber webhook of {viber.BotName} Bot");
    // Set Webhook
    var event_type = viber.SetWebhook(System.Environment.GetEnvironmentVariable("API_CALLBACK_FROM_VIBER"));
    log.Info($"SetWebhook response from Viber: {event_type}");
    log.Info($"Set Viber webhook of {viber.BotName} Bot");
}
