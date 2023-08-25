using System.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AnubotBackend;

public class VectorRepository
{
    private static readonly HttpClient _client = new();
    private readonly string _collection;

    public VectorRepository(IConfiguration configuration)
    {
        _client.BaseAddress = new Uri(configuration["Qdrant:BaseUrl"]
            ?? throw new SettingsPropertyNotFoundException("Qdrant:BaseUrl is not set"));
        _client.DefaultRequestHeaders.Add("accept", "application/json");
        _collection = configuration["Qdrant:Collection"]
            ?? throw new SettingsPropertyNotFoundException("Qdrant:Collection is not set");
    }

    public async Task<List<string>> Search(List<double> queryVector)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                vector = queryVector,
                limit = 3,
                with_payload = true
            }),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await _client.PostAsync(
            $"/collections/{_collection}/points/search",
            jsonContent);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync() ?? throw new Exception("Qdrant: Response is null");
        JsonNode responseNode = JsonNode.Parse(jsonResponse)!;

        List<string> result = responseNode["result"]!.AsArray()
                                                    .Select(point => point!["payload"]!["text"]!.ToString())
                                                    .ToList();

        return result;
    }
}
