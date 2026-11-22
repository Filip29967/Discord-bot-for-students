using System.Globalization;
using DSharpPlus;
using Newtonsoft.Json.Linq;

namespace ProjektOI;

public class CourseTests
{
    private const string InitMarker = "__init_tests__";
    private readonly DatabaseManager databaseManager;
    private readonly UsosClient usosClient;
    private readonly DiscordClient discordClient;

    public CourseTests(DatabaseManager databaseManager, UsosClient usosClient, DiscordClient discordClient)
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
                try { await CheckUserAsync(user); }
                catch (Exception e) { Console.WriteLine($"Sprawdziany {user}: {Short(e.Message)}"); }
            }
            await Task.Delay(TimeSpan.FromMinutes(7), cancellationToken);
        }
    }

    private async Task CheckUserAsync(ulong discordID)
    {
        bool firstRun = !databaseManager.IsGradeSeen(discordID, InitMarker);
        var json = await usosClient.GetUsersDataAsync(discordID, "services/crstests/participant", new());
        var participant = JObject.Parse(json);
        var tests = participant["tests"] as JObject;
        var terms = participant["terms"] as JObject;
        if (tests == null) return;

        bool allOk = true;
        foreach (var term in tests.Properties())
        {
            bool active = (bool?)terms?[term.Name]?["is_active"] ?? false;
            if (!active) continue;

            foreach (var rootProp in ((JObject)term.Value).Properties())
            {
                var root = rootProp.Value;
                long rootId = (long)root["node_id"]!;
                string course = (string?)root["course_edition"]?["course_name"]?["pl"] ?? "?";
                try
                {
                    await CheckRootAsync(discordID, rootId, course, firstRun);
                }
                catch (Exception e)
                {
                    allOk = false;
                    Console.WriteLine($"Sprawdziany {discordID}, root {rootId}: {Short(e.Message)}");
                }
            }
        }

        if (firstRun && allOk) databaseManager.MarkGradeSeen(discordID, InitMarker);
    }

    private async Task CheckRootAsync(ulong discordID, long rootId, string course, bool firstRun)
    {
        var treeJson = await usosClient.GetUsersDataAsync(discordID, "services/crstests/node2", new()
        {
            ["node_id"] = rootId.ToString(),
            ["fields"] = "id|name|type|subnodes_deep"
        });
        var names = new Dictionary<long, string>();
        CollectTasks(JObject.Parse(treeJson), names);
        if (names.Count == 0) return;

        var pointsJson = await usosClient.GetUsersDataAsync(discordID, "services/crstests/user_points", new()
        {
            ["node_ids"] = string.Join("|", names.Keys)
        });

        foreach (var point in JArray.Parse(pointsJson))
        {
            var key = "pt|" + point.ToString(Newtonsoft.Json.Formatting.None);
            if (databaseManager.IsGradeSeen(discordID, key)) continue;

            if (!firstRun)
            {
                long nodeId = (long)point["node_id"]!;
                string task = names.GetValueOrDefault(nodeId, "?");
                double? pts = (double?)point["points"];
                string? comment = (string?)point["comment"];
                var msg = $"Nowa ocena z ({course}): {task} - {pts?.ToString("0.##", CultureInfo.InvariantCulture)} pkt";
                if (!string.IsNullOrWhiteSpace(comment)) msg += $"\nKomentarz: {comment}";

                var user = await discordClient.GetUserAsync(discordID);
                var dm = await user.CreateDmChannelAsync();
                await dm.SendMessageAsync(msg);
            }
            databaseManager.MarkGradeSeen(discordID, key);
        }
    }

    private static void CollectTasks(JToken node, Dictionary<long, string> result)
    {
        if ((string?)node["type"] == "task")
            result[(long)node["id"]!] = (string?)node["name"]?["pl"] ?? "?";
        if (node["subnodes_deep"] is JArray children)
            foreach (var child in children) CollectTasks(child, result);
    }

    private static string Short(string s) => s.Length > 300 ? s[..300] : s;
}