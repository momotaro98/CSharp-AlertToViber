#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Table;
using System;

public static void Run(string triggerQueue, IQueryable<BotUser> inputTable, CloudTable outputTable, TraceWriter log)
{
    string userIdStr = triggerQueue;

    log.Info($"C# Queue trigger function processed with UserId: {userIdStr}");

    List<BotUser> userToBeDeleted = inputTable.Where(u => u.UserId == userIdStr).ToList();
    if (userToBeDeleted.Count == 0)
    {
        log.Info($"Error: UserId, {userIdStr} was not found in BotUser table");
        log.Info("Process End");
        return;
    }

    // Basically, "BotUser" table should not have Entity of duplicated "UserId".
    // However, if it exists, it is necessary to delete all duplicate Entities.
    // So that's why the program uses List of "BotUser" entities.
    foreach (BotUser user in userToBeDeleted)
    {
        log.Info($"Deleting {user.UserName}, UserId: {user.UserId}");
        var operation = TableOperation.Delete(user);
        outputTable.Execute(operation);
        log.Info($"Deleted {user.UserName}, UserId: {user.UserId}");
    }
}

public class BotUser : TableEntity
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}