using DSharpPlus;
namespace ProjektOI;

class Program
{
    static async Task Main(string[] args)
    {
        JSONReader reader = new JSONReader();
        await reader.ReadJSON();
        var builder = DiscordClientBuilder.CreateDefault(reader.Token,
            DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers
        );
        var disocordClient = builder.Build();
        await disocordClient.ConnectAsync();
        await Task.Delay(-1);
    }
}