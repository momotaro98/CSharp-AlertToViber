#load "..\ViberApiLib\Api.csx"

using System;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");

    log.Info($"Setting Viber webhook of {viber.BotName} Bot");
    // Set Webhook
    var event_type = viber.SetWebhook(System.Environment.GetEnvironmentVariable("API_CALLBACK_FROM_VIBER"));
    log.Info($"SetWebhook response from Viber: {event_type}");
    log.Info($"Set Viber webhook of {viber.BotName} Bot");
}
