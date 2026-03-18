using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities;
using HotelSystemBackend.Data;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace HotelSystemBackend.Services;

public interface IAuthServices
{
    // Fix: Return TokenResponseDto, not TokenServices
    Task<(UserDto? user, string? error)> Register(UserForm userForm);
    Task<(TokenResponseDto? tokenResponse, string? error)> Login(string username, string password);
}

public class AuthServices : IAuthServices
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenServices _tokenServices; // Use Interface here for dependency injection best practices

    // Fix 1: Constructor assignment
    public AuthServices(ApplicationDbContext context, ITokenServices tokenServices)
    {
        _context = context;
        _tokenServices = tokenServices; // Corrected assignment
    }
    
    public async Task<(UserDto? user, string? error)> Register(UserForm req)
    {
        if(await _context.Users.AnyAsync(u => u.Username == req.Username))
        {
            return (null, "Username already exists.");
        }
        else if(await _context.Users.AnyAsync(u => u.Email == req.Email))
        {
            return (null, "Email already registered.");
        }

        // Note: Make sure you handle salt if using a specific BCrypt version, 
        // but generally HashPassword handles salt generation automatically.
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);
        
        var user = req.Adapt<User>();
        user.PasswordHash = hashedPassword;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var response = user.Adapt<UserDto>();
        return (response, null);
    }
    

    public async Task<(TokenResponseDto? tokenResponse, string? error)> Login(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return (null, "Invalid username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return (null, "Invalid username or password.");
        }

      
        var userDto = user.Adapt<UserDto>();

    
        var accessToken = _tokenServices.CreateToken(userDto);


        var refreshToken = await _tokenServices.GenerateAndSaveRefreshTokenAsync(user.Id);

 
        var response = new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return (response, null);
    }
}