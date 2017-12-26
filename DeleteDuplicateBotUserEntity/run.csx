#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Table;
using System;

public static void Run(TimerInfo myTimer, IQueryable<BotUser> inputTable, CloudTable outputTable, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    // Set Lists
    List<string> userIdStock = new List<string>();
    List<BotUser> userToBeDeleted = new List<BotUser>();

    // Search if there is duplicate user
    var viberAlertBotName = System.Environment.GetEnvironmentVariable("BOTUSER_TABLE_PARTITIONKEY_VALUE") ?? "";
    foreach (BotUser user in inputTable.Where(u => u.PartitionKey == viberAlertBotName).ToList())
    {
        if (userIdStock.Contains(user.UserId))
        {
            userToBeDeleted.Add(user);
            continue;
        }
        userIdStock.Add(user.UserId);
    }

    // Delete duplicate user entity
    foreach (BotUser user in userToBeDeleted)
    {
        log.Info($"Deleting {user.UserName}, because this user is duplicate on Table Storage.");
        var operation = TableOperation.Delete(user);
        outputTable.Execute(operation);
    }
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}