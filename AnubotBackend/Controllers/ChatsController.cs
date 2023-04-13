using AnubotBackend.Dto;
using AnubotBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnubotBackend.Controllers;

/// <summary>
/// 대화 컨트롤러
/// </summary>
[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly Context _context;

    /// <summary>
    /// 컨트롤러 생성자
    /// </summary>
    /// <param name="context"></param>
    public ChatsController(Context context)
    {
        _context = context;
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
        User? user = _context.Find<User>(dto.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var chat = new Chat()
        {
            Message = dto.Message,
            Reply = "example here!",
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
