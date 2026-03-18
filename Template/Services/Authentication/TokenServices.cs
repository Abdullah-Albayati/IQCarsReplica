using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Template.Data;
using Template.DTOs;

namespace Template.Services;

public interface ITokenServices
{
    string CreateToken(UserDto user);
    string GenerateRefreshToken();
    Task<string> GenerateAndSaveRefreshTokenAsync(int userId);
}

public class TokenServices : ITokenServices
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public TokenServices(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public string CreateToken(UserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Email, user.Email),
        };

        var signingKey = _configuration["AppSettings:Token"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("AppSettings:Token is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = creds,
            Issuer = _configuration["AppSettings:Issuer"],
            Audience = _configuration["AppSettings:Audience"],
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        var refreshToken = GenerateRefreshToken();

        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found when generating refresh token.");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        await _context.SaveChangesAsync();

        return refreshToken;
    }
}

