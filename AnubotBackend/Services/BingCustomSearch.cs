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

        _crawlingClient = new HttpClient();
    }

    /// <summary>
    /// Bing 검색 엔진을 사용하여 주어진 질의에 대한 검색 결과를 가져옵니다.
    /// </summary>
    public async Task<List<string>> SearchAsync(string query)
    {
        using HttpResponseMessage apiResponse = await _apiClient.GetAsync($"?customconfig={_customConfigurationId}&q={query}&mkt=ko-KR&count=1&setLang=ko-KR");
        apiResponse.EnsureSuccessStatusCode();

        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
        JsonNode jsonNode = JsonNode.Parse(jsonResponse)!;

        var webpages = jsonNode["webPages"]!["value"]!.AsArray();

        List<string> relatedDocuments = new(webpages.Count);
        foreach (var webpage in webpages)
        {
            string url = webpage!["url"]!.ToString();
            using HttpResponseMessage crawlingResponse = await _crawlingClient.GetAsync(url);
            crawlingResponse.EnsureSuccessStatusCode();

            string html = await crawlingResponse.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);

            var body = htmlDocument.DocumentNode.SelectSingleNode("//body");
            string bodyText = body.InnerText;
            relatedDocuments.Add(bodyText);
        }

        return relatedDocuments;
    }
}