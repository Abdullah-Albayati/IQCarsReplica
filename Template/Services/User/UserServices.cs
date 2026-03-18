using Mapster;
using Microsoft.EntityFrameworkCore;
using Template.Data;
using Template.DTOs;
using Template.Entities.User;

namespace Template.Services;

public interface IUserServices
{
    Task<(List<UserDto>? users, int? totalCount, string? error)> GetAll(UserFilter filter);
    Task<(UserDto? user, string? error)> Update(int id, UserUpdate userUpdate);
    Task<(UserDto? user, string? error)> Delete(int id);
    Task<(UserDto? user, string? error)> Get(int id);
    Task<(int count, string? error)> DeleteAll();
}

public class UserServices : IUserServices
{
    private readonly ApplicationDbContext _context;

    public UserServices(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<UserDto>? users, int? totalCount, string? error)> GetAll(UserFilter filter)
    {
        var query = _context.Users.AsNoTracking();
        var totalCount = await query.CountAsync();

        if (totalCount == 0)
        {
            return (null, 0, "No users found");
        }

        var users = await query
            .OrderBy(u => u.Id)
            .Skip(filter.GetSafeSkip())
            .Take(filter.GetSafeTake())
            .ToListAsync();

        return (users.Adapt<List<UserDto>>(), totalCount, null);
    }

    public async Task<(UserDto? user, string? error)> Update(int id, UserUpdate userUpdate)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return (null, "User not found");
        }

        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(userUpdate.Username))
        {
            user.Username = userUpdate.Username;
        }

        if (!string.IsNullOrWhiteSpace(userUpdate.Email))
        {
            user.Email = userUpdate.Email;
        }

        if (!string.IsNullOrWhiteSpace(userUpdate.Role)
            && Enum.TryParse<User.UserRoles>(userUpdate.Role, ignoreCase: true, out var role))
        {
            user.Role = role;
        }

        await _context.SaveChangesAsync();
        return (user.Adapt<UserDto>(), null);
    }

    public async Task<(int count, string? error)> DeleteAll()
    {
        var deletedCount = await _context.Users.ExecuteDeleteAsync();
        return (deletedCount, null);
    }

    public async Task<(UserDto? user, string? error)> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return (null, "User not found");
        }

        var dto = user.Adapt<UserDto>();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return (dto, null);
    }

    public async Task<(UserDto? user, string? error)> Get(int id)
    {
        var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return (null, "User not found");
        }

        return (user.Adapt<UserDto>(), null);
    }
}

