using DSharpPlus.Commands;

namespace ProjektOI;

public class TestCommands
{
    [Command("ping")]
    public async Task ExecutePing(CommandContext context,string? name) // ? sprawdza czy null
    {
            await context.RespondAsync($"Pong! {name}");
            
    }
}