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
    private readonly BingSearchEngine _searchEngine;
    private readonly OpenAIService _service;
    private readonly VectorRepository _vectorRepository;

    public ChatsController(BingSearchEngine searchEngine, OpenAIService service, VectorRepository vectorRepository)
    {
        _searchEngine = searchEngine;
        _service = service;
        _vectorRepository = vectorRepository;
    }

    [HttpPost("search-engine")]
    public async Task<ActionResult<Chat>> ChatWithSearchEngine(CreateChatDto dto)
    {
        string searchEngineQuery = await CreateBingQuery(dto.Message);

        List<string> relatedDocuments = await _searchEngine.SearchAsync(searchEngineQuery);

        string systemPrompt = $"""
            Use the provided documents delimited by '===' to answer the user's question.
            If you don't know the answer, just answer 'I don't know'.
            Response in Korean.
            ===
            {relatedDocuments[0]}
            ===
            """;

        var response = await _service.ChatCompletion.CreateCompletion(new()
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
    /// 대화 개체를 생성합니다.
    /// </summary>
    /// <param name="dto">대화 개체 생성 요청 DTO</param>
    [HttpPost("vector-db")]
    public async Task<ActionResult<Chat>> Create(CreateChatDto dto)
    {
        var embeddingResult = await _service.Embeddings.CreateEmbedding(new EmbeddingCreateRequest()
        {
            Model = "text-embedding-ada-002",
            Input = dto.Message,
        });
        if (!embeddingResult.Successful)
        {
            return StatusCode(500, embeddingResult.Error);
        }

        List<double> queryVector = embeddingResult.Data.First().Embedding;

        List<string> relatedDocuments = await _vectorRepository.Search(queryVector);

        StringBuilder systemPromptBuilder = new("당신은 안동대학교 학생들의 질문에 대답해주는 아누봇입니다.");
        foreach (string document in relatedDocuments)
        {
            systemPromptBuilder.AppendLine(document);
        }

        var result = await _service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>()
            {
                ChatMessage.FromSystem(systemPromptBuilder.ToString()),
                ChatMessage.FromUser(dto.Message),
            },
        });
        if (!result.Successful)
        {
            return StatusCode(500, result.Error);
        }

        var chat = new Chat()
        {
            Message = dto.Message,
            Reply = result.Choices.First().Message.Content
        };

        return Ok(chat);
    }

    /// <summary>
    /// GPT 언어 모델을 사용하여, 사용자 프롬프트에 대응하는 검색 엔진 쿼리를 생성합니다.
    /// </summary>
    /// <param name="userPrompt">사용자 프롬프트</param>
    /// <returns>사용자 프롬프트에 대응하는 검색 엔진 쿼리</returns>
    private async Task<string> CreateBingQuery(string userPrompt)
    {
        const string SYSTEM_PROMPT = """
            You are good at using search engine.
            You must extract search keyword matching to user's question.
            Just answer with only a search keyword.
            """;

        var response = await _service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = new ChatMessage[]
            {
                ChatMessage.FromSystem(SYSTEM_PROMPT),
                ChatMessage.FromUser(userPrompt),
            },
            Temperature = 0F,
        });

        return response.Choices.First().Message.Content;
    }
}
