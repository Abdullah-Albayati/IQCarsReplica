using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelSystemBackend.Data;
using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities;
using HotelSystemBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mapster;

namespace HotelSystemBackend.Controllers
{
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



        [HttpGet("me")]
        public IActionResult GetMeDebug()
        {
            // If the user isn't authenticated, this will be false
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("Server says: User is not authenticated.");
            }

            // Return ALL claims so you can see the exact keys
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] UserForm req)
        {
            var (user, error) = await _authServices.Register(req);
            if (error != null)
            {
                return BadRequest(error);
            }

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(string username, string password)
        {
            var (tokenResponse, error) = await _authServices.Login(username, password);

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);


            SetRefreshTokenCookie(tokenResponse!.RefreshToken);


            return Ok(new { token = tokenResponse.AccessToken });
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };
            HttpContext.Response.Cookies.Delete("refreshToken", cookieOptions);
            HttpContext.Response.Cookies.Delete("token", cookieOptions); // If you use this
            //HttpContext.Session.Clear();
            return Ok(new { message = "User successfully logged out" });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            // 1. Get the refresh token from the cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token provided");

            // 2. Validate the token against the database
            // You'll need to find the user associated with this token
            var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            // 3. Generate new tokens (Rotation)
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
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}