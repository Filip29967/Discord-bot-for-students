using System.Security.Cryptography;
using System.Text;
using DSharpPlus;
using Newtonsoft.Json.Linq;

namespace ProjektOI;

public class Grades
{
    private DatabaseManager databaseManager;
    private UsosClient usosClient;
    private DiscordClient discordClient;
    
    public Grades(DatabaseManager databaseManager,UsosClient usosClient,DiscordClient discordClient)
    {
        this.databaseManager = databaseManager;
        this.usosClient = usosClient;
        this.discordClient = discordClient;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var user in databaseManager.SelectAllUsers())
            {
                try
                {
                    await CheckUserAsync(user);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{user}{e.Message}");
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(7),cancellationToken);
        }
    }

    private async Task CheckUserAsync(ulong discordID)
    {
       
        
        bool firstRun = !databaseManager.CheckIfUserHasGrade(discordID);
        var json = await usosClient.GetUsersDataAsync(discordID, "services/grades/latest", new()
        {
            ["fields"] = "value_symbol|passes|value_description|exam_id|exam_session_number|comment|date_modified|course_edition"
        });
        var grades = JArray.Parse(json);

        foreach (var grade in grades)
        {
            var hash = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(grade.ToString(Newtonsoft.Json.Formatting.None))));

            if (databaseManager.IsGradeSeen(discordID, hash))
            {
                continue;
            }

            if (!firstRun)
            {
                var user = await discordClient.GetUserAsync(discordID);
                var dm = await user.CreateDmChannelAsync();
                string course = (string?)grade["course_edition"]?["course_name"]?["pl"] ?? "nieznany przedmiot";
                string symbol = (string?)grade["value_symbol"] ?? "?";
                string desc = (string?)grade["value_description"]?["pl"] ?? "";
                string? comment = (string?)grade["comment"];
                var msg = $"Nowa ocena końcowa z przedmiotu {course}: {symbol} {desc}".Trim();
                if (!string.IsNullOrWhiteSpace(comment)) msg += $"\nKomentarz: {comment}";
                await dm.SendMessageAsync(msg);
            }
            databaseManager.MarkGradeSeen(discordID, hash);
        }

        if (firstRun)
        {
            databaseManager.MarkGradeSeen(discordID, "__init__");
        }
    }
}