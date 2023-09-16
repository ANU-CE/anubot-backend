using HtmlAgilityPack;
using System.Text.Json.Nodes;

namespace AnubotBackend.Services;

public class BingCustomSearch
{
    private readonly HttpClient _apiClient;
    private readonly HttpClient _crawlingClient;
    private readonly string _customConfigurationId;

    public BingCustomSearch(IConfiguration configuration)
    {
        string apiKey = configuration["BingSearch:ApiKey"]
            ?? throw new Exception("BingSearch:ApiKey is not set.");
        _customConfigurationId = configuration["BingSearch:CustomConfigurationId"]
            ?? throw new Exception("BingSearch:CustomConfigurationId is not set.");

        _apiClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.bing.microsoft.com/v7.0/custom/search"),
            DefaultRequestHeaders = {
                { "Ocp-Apim-Subscription-Key", apiKey }
            }
        };

        _crawlingClient = new HttpClient()
        {
            DefaultRequestHeaders = {
                { "User-Agent", "AnubotBackend" }
            }
        };
    }

    /// <summary>
    /// Bing 검색 엔진을 사용하여 주어진 질의에 대한 검색 결과를 가져옵니다.
    /// </summary>
    public async Task<string?> SearchAsync(string query)
    {
        using HttpResponseMessage apiResponse = await _apiClient.GetAsync($"?customconfig={_customConfigurationId}&q={query}&mkt=ko-KR&count=1&setLang=ko-KR");
        apiResponse.EnsureSuccessStatusCode();

        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        JsonNode jsonNode = JsonNode.Parse(jsonResponse)!;

        var bestMatch = jsonNode["webPages"]!["value"]![0]!;

        string url = bestMatch["url"]!.ToString();

        using HttpResponseMessage crawlingResponse = await _crawlingClient.GetAsync(url);
        if (!crawlingResponse.IsSuccessStatusCode)
        {
            return null;
        }

        string html = await crawlingResponse.Content.ReadAsStringAsync();
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(html);

        var body = htmlDocument.DocumentNode.SelectSingleNode("//body");
        if (body is null)
        {
            return null;
        }

        string bodyText = body.InnerText;

        // bodyText에서 불필요한 문자들을 제거하여 토큰수를 절약합니다.
        bodyText = bodyText.Replace("\n", string.Empty);
        bodyText = bodyText.Replace("\r", string.Empty);
        bodyText = bodyText.Replace("\t", string.Empty);

        return bodyText;
    }
}