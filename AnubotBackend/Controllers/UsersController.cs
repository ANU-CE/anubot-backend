using AnubotBackend.Dto;
using AnubotBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnubotBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly Context _context;

    public UsersController(Context context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(CreateUserDto dto)
    {
        var user = new User()
        {
            Name = dto.UserName,
            CreatedDateTime = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }
}
