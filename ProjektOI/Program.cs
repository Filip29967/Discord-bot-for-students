using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace ProjektOI;

class Program
{
    static async Task Main(string[] args)
    {
        JSONReader reader = new JSONReader(); 
        await reader.ReadJSON(); //odczyt parametrow z pliku json
        DatabaseManager db = new DatabaseManager();
        db.CreateDatabase();
        // Tworzenie klienta
        var builder = DiscordClientBuilder.CreateDefault(
            reader.Token,
            DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers
        );
        //do konstruktorow klas z komendami jestem w stanie tak przekazywac argumenty
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(db);
            services.AddSingleton(reader);
        });
        // wlaczenie obslugi komend
        builder.UseCommands((provider, extension) =>
        {
            extension.AddCommands([typeof(AuthCommands),typeof(TaskCommands)]);
            var slashCommandProcessor = new SlashCommandProcessor(new SlashCommandConfiguration());
            extension.AddProcessor(slashCommandProcessor);
        });
        // tworzenie klienta ze wszystkim atrybutami dodanymi mu wczesniej jak np obsluga komend
        var disocordClient = builder.Build();
        // connect z serwerem discord
        await disocordClient.ConnectAsync();
        var usos = new UsosClient(db,reader);
        _ = Task.Run(() => new Grades(db, usos, disocordClient).RunAsync());
        _ = Task.Run(() => new CourseTests(db, usos, disocordClient).RunAsync());
        var reminder = new ReminderManager(db, disocordClient);
        _ = Task.Run(() => reminder.SendReminderAsync());
        //dzialaj w nieskonczonsc
        await Task.Delay(-1);
    }
}