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

        string? relatedDocument = await _bingCustomSearch.SearchAsync(bingQuery);

        string systemPrompt =
        $"""
        당신은 안동대학교 학생들을 도와주는 인공지능 비서 아누봇입니다.
        대화가 발생한 시각은 UTC 시간으로 {DateTime.UtcNow}입니다.
        "==="로 구분된 부분을 참고하여 학생의 질문에 답변해주세요.
        만약 답변이 불가능하다면 학생에게 다른 방법을 제시해주세요.
        ===
        {relatedDocument}
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
        사용자의 질문에 대한 참고자료를 찾기 위한 한 개의 검색어를 만드세요.
        """;

        var response = await _openAiService.ChatCompletion.CreateCompletion(new()
        {
            Model = "gpt-3.5-turbo",
            Messages = new ChatMessage[]
            {
                ChatMessage.FromSystem(SYSTEM_PROMPT),
                ChatMessage.FromUser(userPrompt),
            },
        });

        string query = response.Choices.First().Message.Content;

        // GPT가 생성한 검색어의 경우 따옴표와 같은 연산자 때문에 검색 성능이 떨어지는 경우가 있습니다.
        // 따라서 프로그램 상에서 따옴표 연산자를 제거합니다.
        query = query.Replace("\"", string.Empty);

        return query;
    }
}
