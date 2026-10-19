using Newtonsoft.Json;

namespace ProjektOI;

public class JSONReader
{
    public string Token { get; set; }

    public async Task ReadJSON()
    {
        using (StreamReader reader = new StreamReader("config.json"))
        {
            string json = await reader.ReadToEndAsync();
            JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);
            Token = data.Token;
        }
    }
}