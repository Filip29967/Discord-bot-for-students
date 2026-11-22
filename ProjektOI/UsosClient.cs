using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace ProjektOI;

public class UsosClient
{
    private JSONReader reader;
    private DatabaseManager databaseManager;

    public UsosClient(DatabaseManager databaseManager, JSONReader reader)
    {
        this.databaseManager = databaseManager;
        this.reader = reader;
    }

    public async Task<string> GetUsersDataAsync(ulong discordID, string path, Dictionary<string, string> parameters)
    {
        var (token,secret) = databaseManager.SelectUsersData(discordID);
        if (token == null)
        {
            throw new InvalidOperationException("Brak autoryzacji");
            
        }

        var options = new RestClientOptions("https://usosapi.polsl.pl/")
        {
            Authenticator =
                OAuth1Authenticator.ForProtectedResource(reader.CustomerKey, reader.CustomerSecret, token, secret)
        };
        var client = new RestClient(options);
        var request = new RestRequest(path, Method.Post);
        foreach (var p in parameters)
        {
            request.AddParameter(p.Key, p.Value);
        }
        var response = await client.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            throw new Exception($"Błąd podczas pobierania danych: {response.StatusCode} - {response.Content}");
        }
        return response.Content!;
    }
}