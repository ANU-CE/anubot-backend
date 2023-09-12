using AnubotBackend.Dto;
using AnubotBackend.Models;
using AnubotBackend.Services;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Text;

namespace AnubotBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly BingCustomSearch _bingCustomSearch;
    private readonly OpenAIService _openAiService;
    private readonly VectorRepository _vectorRepository;

    public ChatsController(BingCustomSearch bingCustomSearch, OpenAIService openAiService, VectorRepository vectorRepository)
    {
        _bingCustomSearch = bingCustomSearch;
        _openAiService = openAiService;
        _vectorRepository = vectorRepository;
    }

    /// <summary>
    /// Bing Custom Search를 사용하여 응답을 생성합니다.
    /// </summary>
    /// <param name="dto">대화 개체 생성 요청 DTO</param>
    [HttpPost("Bing")]
    public async Task<ActionResult<Chat>> ChatWithBing(CreateChatDto dto)
    {
        string bingQuery = await CreateBingQuery(dto.Message);

        List<string> relatedDocuments = await _bingCustomSearch.SearchAsync(bingQuery);

        string systemPrompt =
        $"""
        Use the provided documents delimited by '===' to answer the user's question.
        If you don't know the answer, just answer 'I don't know'.
        You are fluent in Korean.
        ===
        {string.Join("\n", relatedDocuments)}
        ===
        """;

        var response = await _openAiService.ChatCompletion.CreateCompletion(new()
        {
            Model = "gpt-3.5-turbo-16k",
            Messages = new List<ChatMessage>()
            {
                ChatMessage.FromSystem(systemPrompt),
                ChatMessage.FromUser(dto.Message),
            },
        });

        Chat chat = new()
        {
            Message = dto.Message,
            Reply = response.Choices.First().Message.Content
        };

        return Ok(chat);
    }

    /// <summary>
    /// 임베딩 기반 검색을 통해 응답을 생성합니다.
    /// </summary>
    /// <param name="dto">대화 개체 생성 요청 DTO</param>
    [HttpPost("Vector")]
    public async Task<ActionResult<Chat>> ChatWithVector(CreateChatDto dto)
    {
        var embeddingResponse = await _openAiService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest()
        {
            Model = "text-embedding-ada-002",
            Input = dto.Message,
        });

        List<double> embedding = embeddingResponse.Data.First().Embedding;

        List<string> relatedDocuments = await _vectorRepository.Search(embedding);

        StringBuilder systemPromptBuilder = new("당신은 안동대학교 학생들의 질문에 대답해주는 아누봇입니다.");
        foreach (string document in relatedDocuments)
        {
            systemPromptBuilder.AppendLine(document);
        }

        var response = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>()
            {
                ChatMessage.FromSystem(systemPromptBuilder.ToString()),
                ChatMessage.FromUser(dto.Message),
            },
        });

        Chat chat = new()
        {
            Message = dto.Message,
            Reply = response.Choices.First().Message.Content
        };

        return Ok(chat);
    }

    /// <summary>
    /// GPT 언어 모델을 사용하여, 사용자 프롬프트에 대응하는 Bing 쿼리를 생성합니다.
    /// </summary>
    /// <param name="userPrompt">사용자 프롬프트</param>
    /// <returns>사용자 프롬프트에 대응하는 검색 엔진 쿼리</returns>
    private async Task<string> CreateBingQuery(string userPrompt)
    {
        const string SYSTEM_PROMPT =
        """
        Create a query for Bing Custom Search based on the user's prompt.
        You can use advanced search options for precise search results.
        Example: "컴퓨터교육과 연락처 알려줘" -> "컴퓨터교육과 연락처"
        """;

        var response = await _openAiService.ChatCompletion.CreateCompletion(new()
        {
            Model = "gpt-3.5-turbo",
            Messages = new ChatMessage[]
            {
                ChatMessage.FromSystem(SYSTEM_PROMPT),
                ChatMessage.FromUser(userPrompt),
            },
            Temperature = 0F,
        });

        string query = response.Choices.First().Message.Content;

        return query;
    }
}
