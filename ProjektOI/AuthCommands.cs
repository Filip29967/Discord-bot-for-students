using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands;

namespace ProjektOI;

public class AuthCommands
{
    /*
    flow co gdzie leci w kodzie po kolei
    uzytkoniwk wpisuje /connect
    wywoluje sie metoda Connect w tej klasie
    w authenticatorze wykonuje sie komenda GetRequestTokenAsync ktora zapisuje request tokeny w bazie
    z bazy tokeny wracaja znowu do connect i uzytkownik dostaje link do drugiego etapu autoryzacji
    uzytkownik otrzymuje pin i wpisuje go uzywajc /pin
    w authenticatorze wykonuje sie komenda GetAccessTokenAsync ktora zapisuje access tokeny w bazie
    aplikacja ma dostep do planu uzytkonwika
     */
    private Authenticator authenticator;
    private DatabaseManager databaseManager;
    public AuthCommands(DatabaseManager databaseManager,JSONReader jsonReader)
    {
        authenticator = new Authenticator(jsonReader.CustomerKey, jsonReader.CustomerSecret, "https://usosapi.polsl.pl/",databaseManager);
        this.databaseManager = databaseManager;
    }
    
    [Command("connect")]
    public async Task ExecuteConnect(SlashCommandContext context)
    {
        // do authenticator idzie request po token tam ten token leci do bazy danych i z bazy wraca tu
        await context.DeferResponseAsync(true);
        try
        {
            await authenticator.GetRequestTokenAsync(context.User.Id); 
            var (requestToken, _) = databaseManager.SelectUsersRequest(context.User.Id);
            await context.RespondAsync("Wejdź na podaną stronę, zaloguj i skopiuj kod pin. Następnie użyj komendy /pin i wpisz go jako argument");
            await context.RespondAsync($"https://usosapi.polsl.pl/services/oauth/authorize?oauth_token={requestToken}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await context.RespondAsync("błąd połączenia");
        }
        
    }
    [Command("pin")]
    public async Task ExecutePin(SlashCommandContext context, string pin)
    {
        await context.DeferResponseAsync(true);
        try
        {
            var (requestToken, requestTokenSecret) = databaseManager.SelectUsersRequest(context.User.Id);
            await authenticator.GetAccessTokenAsync(requestToken, requestTokenSecret, pin, context.User.Id);
            await context.RespondAsync("Autoryzacja przeprowadzona poprawnie");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}