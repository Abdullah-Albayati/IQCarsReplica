using Mapster;
using Microsoft.EntityFrameworkCore;
using Template.Data;
using Template.DTOs;
using Template.Entities.User;

namespace Template.Services;

public interface IAuthServices
{
    Task<(UserDto? user, string? error)> Register(UserForm userForm);
    Task<(TokenResponseDto? tokenResponse, string? error)> Login(string username, string password);
}

public class AuthServices : IAuthServices
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenServices _tokenServices;

    public AuthServices(ApplicationDbContext context, ITokenServices tokenServices)
    {
        _context = context;
        _tokenServices = tokenServices;
    }

    public async Task<(UserDto? user, string? error)> Register(UserForm req)
    {
        if (await _context.Users.AnyAsync(u => u.Username == req.Username))
        {
            return (null, "Username already exists.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == req.Email))
        {
            return (null, "Email already registered.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = req.Adapt<User>();
        user.PasswordHash = hashedPassword;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (user.Adapt<UserDto>(), null);
    }

    public async Task<(TokenResponseDto? tokenResponse, string? error)> Login(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
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

