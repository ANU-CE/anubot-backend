using AnubotBackend.Dto;
using AnubotBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnubotBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly Context _context;

    public ChatsController(Context context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Chat>> Get(Guid id)
    {
        Chat? chat = await _context.Chats.FindAsync(id);
        if (chat == null)
        {
            return NotFound();
        }

        return chat;
    }

    [HttpPost]
    public async Task<ActionResult<Chat>> Create(CreateChatDto dto)
    {
        var chat = new Chat()
        {
            Message = dto.Message,
            Reply = "example here!",
            UserId = dto.UserId,
            CreatedDateTime = DateTime.UtcNow
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = chat.Id }, chat);
    }

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
