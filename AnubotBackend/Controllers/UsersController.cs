using AnubotBackend.Dto;
using AnubotBackend.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AnubotBackend.Controllers;

/// <summary>
/// 유저 컨트롤러
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly Context _context;

    /// <summary>
    /// 유저 컨트롤러 생성자
    /// </summary>
    public UsersController(Context context)
    {
        _context = context;
    }

    /// <summary>
    /// 유저 ID로 유저 개체를 가져옵니다.
    /// </summary>
    /// <param name="id">찾을 유저의 ID</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        string? currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (id != Guid.Parse(currentUserId))
        {
            return Unauthorized();
        }

        User? user = await _context.Users
            .Include(u => u.Chats)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    /// <summary>
    /// 유저 개체를 생성합니다.
    /// </summary>
    /// <param name="dto">유저 개체 생성 요청 DTO</param>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<User>> Create(CreateUserDto dto)
    {
        string googleId;
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleIdToken);
            googleId = payload.Subject;
        }
        catch (InvalidJwtException)
        {
            return BadRequest();
        }

        var user = new User()
        {
            Name = dto.UserName,
            CreatedDateTime = DateTime.UtcNow,
            GoogleId = googleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }
}
