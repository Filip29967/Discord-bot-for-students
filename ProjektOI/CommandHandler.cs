using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ProjektOI;

public class CommandHandler
{
   private Dictionary<string, string> commands = new Dictionary<string, string>()
   {
      ["ping"] = "pong"
   };
   private string prefix;
   public CommandHandler(string prefix)
   {
      this.prefix = prefix;
   }

   public async Task HandleCommand(DiscordClient client, MessageCreatedEventArgs e)
   {
      if (e.Message.Author != null && e.Message.Author.IsBot)
      {
         return;
      }

      if (!e.Message.Content.StartsWith(prefix))
      {
         return;
      }
      string commandWithoutPrefix = e.Message.Content.Substring(prefix.Length);
      string[] commandSplit = commandWithoutPrefix.Split(" ",2);
      string argument;
      if (commandSplit.Length > 1)
      {
         argument = commandSplit[1];
      }
      else
      {
         argument = "";
      }

      switch (commandSplit[0])
      {
         case "a":
            break;
         default:
            if(commands.TryGetValue(commandSplit[0], out string response))
            {
               await e.Message.RespondAsync(response);
            }
            else
            {
               await e.Message.RespondAsync("Unknown command");
            }
            break;
      }
   }
}