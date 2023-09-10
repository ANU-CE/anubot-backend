using AnubotBackend.Dto;
using AnubotBackend.Models;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Text;

namespace AnubotBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly OpenAIService _service;
    private readonly VectorRepository _vectorRepository;

    public ChatsController(OpenAIService service, VectorRepository vectorRepository)
    {
        _service = service;
        _vectorRepository = vectorRepository;
    }

    /// <summary>
    /// 대화 개체를 생성합니다.
    /// </summary>
    /// <param name="dto">대화 개체 생성 요청 DTO</param>
    [HttpPost]
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
}
