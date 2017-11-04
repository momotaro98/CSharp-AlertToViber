#load "..\ViberApiLib\Api.csx"

using System;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    // Use ViberApi
    Api viber = new Api(System.Environment.GetEnvironmentVariable("VIBER_AUTH_TOKEN"), "momotaroBot", "");
    var name = viber.GetName();
    log.Info(name);

    var event_type = viber.SetWebhook(System.Environment.GetEnvironmentVariable("API_CALLBACK_FROM_VIBER"));
    log.Info(event_type);
}
