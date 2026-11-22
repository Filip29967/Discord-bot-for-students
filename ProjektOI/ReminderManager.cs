using DSharpPlus;
namespace ProjektOI;

public class ReminderManager
{
    private DatabaseManager databaseManager;
    private DiscordClient discordClient;
    public ReminderManager(DatabaseManager databaseManager,DiscordClient discordClient)
    {
        this.databaseManager = databaseManager;
        this.discordClient = discordClient;
    }

    public async Task SendReminderAsync(CancellationToken cancellationToken = default)
    {
        var timInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var task in databaseManager.SelectUserTasks())
            {
                try
                {
                    var channel = await discordClient.GetChannelAsync(task.ChannelId);
                    var deadline = TimeZoneInfo.ConvertTimeFromUtc(task.Deadline, timInfo);
                    await channel.SendMessageAsync($"PRZYPOMNIENIE <@{task.UserId}>: {task.Name} - {deadline}");
                    databaseManager.MarkTaskSent(task.Id);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(20),cancellationToken);
        }
    }
}