using AnubotBackend.Dto;
using AnubotBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Text;

namespace AnubotBackend.Controllers;

/// <summary>
/// 대화 컨트롤러
/// </summary>
[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly OpenAIService _service;
    private readonly Context _context;
    private readonly VectorRepository _vectorRepository;

    /// <summary>
    /// 컨트롤러 생성자
    /// </summary>
    /// <param name="context"></param>
    /// <param name="service"></param>
    /// <param name="vectorRepository"></param>
    public ChatsController(Context context, OpenAIService service, VectorRepository vectorRepository)
    {
        _context = context;
        _service = service;
        _vectorRepository = vectorRepository;
    }

    /// <summary>
    /// 대화 ID로 대화 개체를 가져옵니다.
    /// </summary>
    /// <param name="id">대화 ID</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<Chat>> Get(Guid id)
    {
        Chat? chat = await _context.Chats
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chat == null)
        {
            return NotFound();
        }

        return chat;
    }

    /// <summary>
    /// 대화 개체를 생성합니다.
    /// </summary>
    /// <param name="dto">대화 개체 생성 요청 DTO</param>
    [HttpPost]
    public async Task<ActionResult<Chat>> Create(CreateChatDto dto)
    {
        User? user = await _context.FindAsync<User>(dto.UserId);
        if (user == null)
        {
            return NotFound();
        }

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
            Reply = result.Choices.First().Message.Content,
            User = user,
            CreatedDateTime = DateTime.UtcNow
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = chat.Id }, chat);
    }

    /// <summary>
    /// 대화 개체를 갱신합니다. 아누봇 응답에 대한 사용자 피드백을 전달할 수 있습니다.
    /// </summary>
    /// <param name="id">대화 ID</param>
    /// <param name="dto">대화 개체 갱신 요청 DTO</param>
    [HttpPatch("{id}")]
    public async Task<ActionResult<Chat>> Update(Guid id, UpdateChatDto dto)
    {
        Chat? chat = await _context.Chats.FindAsync(id);
        if (chat == null)
        {
            return NotFound();
        }

        chat.Feedback = dto.Feedback;
        await _context.SaveChangesAsync();
        return chat;
    }
}
