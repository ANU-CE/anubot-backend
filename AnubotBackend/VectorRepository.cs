using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AnubotBackend;

/// <summary>
/// 벡터 데이터베이스
/// </summary>
public class VectorRepository
{
    private static readonly HttpClient _client = new();

    /// <summary>
    /// 벡터 데이터베이스 생성자
    /// </summary>
    /// <param name="config">DB주소와 API 키를 가져올 설정</param>
    public VectorRepository(IConfiguration config)
    {
        _client.BaseAddress = new Uri(config["Qdrant:BaseUrl"] ?? throw new Exception("Qdrant:BaseUri is not set"));
        _client.DefaultRequestHeaders.Add("accept", "application/json");
        _client.DefaultRequestHeaders.Add("api-key", config["Qdrant:ApiKey"] ?? throw new Exception("Qdrant:ApiKey is not set"));
    }

    /// <summary>
    /// 주어진 문서 벡터에 대해 코사인 유사도가 가장 높은 문서 3개를 가져옵니다.
    /// </summary>
    /// <param name="queryVector">기준이 될 문서의 벡터</param>
    /// <returns>코사인 유사도가 가장 높은 문서 3개</returns>
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
            "/collections/contacts/points/search",
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
