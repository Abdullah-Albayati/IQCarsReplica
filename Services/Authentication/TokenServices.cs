using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelSystemBackend.Data;
using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities; // Ensure your User entity is accessible here
using Microsoft.IdentityModel.Tokens;

namespace HotelSystemBackend.Services;

public interface ITokenServices
{
    string CreateToken(UserDto user);
    string GenerateRefreshToken();
    Task<string> GenerateAndSaveRefreshTokenAsync(int userId); // Changed input to userId
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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token life
            SigningCredentials = creds,
            Issuer = _configuration["AppSettings:Issuer"],
            Audience = _configuration["AppSettings:Audience"]
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

       
        var userEntity = await _context.Users.FindAsync(userId);

        if (userEntity == null)
        {
            throw new Exception("User not found when generating refresh token.");
        }

      
        userEntity.RefreshToken = refreshToken;
        userEntity.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}