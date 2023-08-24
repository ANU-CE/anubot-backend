using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AnubotBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly Context _context;
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly SigningCredentials _credentials;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration, Context context)
    {
        _configuration = configuration;
        _context = context;
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
            ?? throw new SettingsPropertyNotFoundException("Jwt:Key is not set.")));
        _credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// API를 호출할때 필요한 JWT를 생성합니다.
    /// </summary>
    /// <param name="googleIdToken">해당 유저의 Google id token</param>
    /// <response code="200">Bearer json web token</response>
    /// <response code="400">유효하지 않은 Google id token</response>
    /// <response code="404">해당 Google id token으로 가입된 유저가 없음</response>
    [AllowAnonymous]
    [HttpPost]
    public ActionResult<string> GenerateToken(string googleIdToken)
    {
        string googleId;
        try
        {
            var payload = GoogleJsonWebSignature.ValidateAsync(googleIdToken).Result;
            googleId = payload.Subject;
        }
        catch (InvalidJwtException)
        {
            return BadRequest();
        }

        var user = _context.Users.Where(u => u.GoogleId == googleId).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new SettingsPropertyNotFoundException("Jwt:Issuer is not set."),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: _credentials);

        return _handler.WriteToken(token);
    }
}