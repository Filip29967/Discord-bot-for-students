using System.ComponentModel;
using System.Globalization;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands;
namespace ProjektOI;

public class TaskCommands
{
    private DatabaseManager databaseManager;

    public TaskCommands(DatabaseManager databaseManager)
    {
        this.databaseManager = databaseManager;
    }
    [Command("task"),Description("Stwórz zadanie do wykonania")]
    public async Task ExecuteTaskAsync(SlashCommandContext context, string name,string deadlline,string reminderr)
    {
        await context.DeferResponseAsync(true);
        try
        {
            var pl = new CultureInfo("pl-PL");
            bool deadlineOK = DateTime.TryParse(deadlline, pl, DateTimeStyles.None, out var deadline );
            bool reminderOK = DateTime.TryParse(reminderr, pl, DateTimeStyles.None, out var reminder);
            if (!deadlineOK || !reminderOK)
            {
                await context.RespondAsync("Niepoprawny format daty");
                return;
            }

            if (reminder > deadline)
            {
                await context.RespondAsync("Czas przypomnienia nie może być późniejszy niż czas wykonania zadania");
                return;
            }
            
           var timInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
           var deadlineUTC = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(deadline, timInfo), DateTimeKind.Utc);
           var reminderUTC = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(reminder, timInfo), DateTimeKind.Utc);
           
           if (reminderUTC <= DateTime.UtcNow)
           {
               await context.RespondAsync("Czas przypomnienia minął");
               return;
           }
           databaseManager.InsertUserTasks(new UserTask
           {
               Name = name,
               Deadline = deadlineUTC,
               ReminderTime = reminderUTC,
               UserId = context.User.Id,
               ChannelId = context.Channel.Id
           });
           await context.RespondAsync($"Dodano zadanie!\nNazwa: {name}\nTermin: {deadline:dd.MM.yyyy HH:mm}\nPrzypomnienie: {reminder:dd.MM.yyyy HH:mm}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            await context.RespondAsync("Wystąpił błąd podczas dodawania zadania.");
        }
    }
}