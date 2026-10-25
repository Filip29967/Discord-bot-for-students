using Newtonsoft.Json;
namespace ProjektOI;

public class JSONReader
{
    public string Token { get; set; }
    public string CustomerKey { get; set; }
    public string CustomerSecret { get; set; }

    public async Task ReadJSON()
    {
        using (StreamReader reader = new StreamReader("config.json"))
        {
            string json = await reader.ReadToEndAsync();
            JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json); // zamienia string z pliku na obikety
            Token = data.Token;
            CustomerKey = data.CustomerKey;
            CustomerSecret = data.CustomerSecret;
        }
    }
}