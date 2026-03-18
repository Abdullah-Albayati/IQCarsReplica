using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Data;
using Template.DTOs;
using Template.Services;

namespace Template.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;
    private readonly ApplicationDbContext _context;
    private readonly ITokenServices _tokenServices;

    public AuthController(IAuthServices authServices, ApplicationDbContext context, ITokenServices tokenServices)
    {
        _authServices = authServices;
        _context = context;
        _tokenServices = tokenServices;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] UserForm req)
    {
        var (user, error) = await _authServices.Register(req);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromQuery] string username, [FromQuery] string password)
    {
        var (tokenResponse, error) = await _authServices.Login(username, password);

        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(error);
        }

        SetRefreshTokenCookie(tokenResponse!.RefreshToken);

        return Ok(new { token = tokenResponse.AccessToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
        };

        HttpContext.Response.Cookies.Delete("refreshToken", cookieOptions);
        return Ok(new { message = "User successfully logged out" });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<object>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("No refresh token provided");
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token");
        }

        var userDto = user.Adapt<UserDto>();
        var newAccessToken = _tokenServices.CreateToken(userDto);
        var newRefreshToken = await _tokenServices.GenerateAndSaveRefreshTokenAsync(user.Id);

        SetRefreshTokenCookie(newRefreshToken);

        return Ok(new { token = newAccessToken });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30),
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}


