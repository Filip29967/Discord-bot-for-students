using RestSharp;
using RestSharp.Authenticators;

namespace ProjektOI;

public class Authenticator
{
    private string customerKey;
    private string customerSecret;
    private string url;
    private DatabaseManager databaseManager;
    
    public Authenticator(string customerKey, string customerSecret,string url,DatabaseManager databaseManager)
    {
        this.customerKey = customerKey;
        this.customerSecret = customerSecret;
        this.url = url;
        this.databaseManager = databaseManager;
    }

    public async Task GetRequestTokenAsync(ulong discordID)
    {
        var requestOptions = new RestClientOptions(url)
        {
            Authenticator = 
                OAuth1Authenticator.ForRequestToken(customerKey, customerSecret, "oob")
        };
        var client = new RestClient(requestOptions);
        var request = new RestRequest("services/oauth/request_token", Method.Post);
        request.AddParameter("scopes","studies|offline_access|grades|crstests");
        var response = await client.ExecuteAsync(request);
        Console.WriteLine(response.StatusCode);
        var requestParsed = System.Web.HttpUtility.ParseQueryString(response.Content);
        var requestToken = requestParsed["oauth_token"];
        var requestTokenSecret = requestParsed["oauth_token_secret"];
        databaseManager.InsertUsersRequest(discordID, requestToken, requestTokenSecret);
    }

    public async Task GetAccessTokenAsync(string requestToken, string requestTokenSecret, string pin, ulong discordID)
    {
        var accesOptions = new RestClientOptions(url)
        {
            Authenticator =
                OAuth1Authenticator.ForAccessToken(customerKey, customerSecret, requestToken, requestTokenSecret, pin)
        };
        var client = new RestClient(accesOptions);
        var request = new RestRequest("services/oauth/access_token", Method.Post);
        var response = await client.ExecuteAsync(request);
        Console.WriteLine(response.StatusCode);
        var accessTokenParsed = System.Web.HttpUtility.ParseQueryString(response.Content);
        var accessToken = accessTokenParsed["oauth_token"];
        var accessTokenSecret = accessTokenParsed["oauth_token_secret"];
        databaseManager.InsertUsersData(discordID, accessToken, accessTokenSecret);
    }
}