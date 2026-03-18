using AutoMapper;
using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities;
using HotelSystemBackend.Data;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HotelSystemBackend.Services;

public interface IUserServices
{

    Task<(List<UserDto> users, int? totalCount, string? error)> GetAll(UserFilter filter);
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

    public async Task<(List<UserDto> users, int? totalCount, string? error)> GetAll(UserFilter filter)
    {
        var users = await _context.Users.ToListAsync();

        if (users.Count == 0)
        {
            return (null, null, "No users found");
        }
        
        var userDtos = users.Adapt<List<UserDto>>();

        var response = new
        {
            users = userDtos,
            totalCount = userDtos.Count,
            error = ""
        };
        return (response.users, response.totalCount, response.error);
    }

    public async Task<(UserDto? user, string? error)> Update(int id, UserUpdate userUpdate)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (null, "User not found");
        user.UpdatedAt = DateTime.UtcNow;
 
        userUpdate.Adapt(user, Mapster.TypeAdapterConfig<UserUpdate, User>
            .NewConfig()
            .IgnoreNullValues(true) 
            .Config);
        await _context.SaveChangesAsync();
        return (user.Adapt<UserDto>(), null);
    }

    public async Task<(int count, string? error)> DeleteAll()
    {
        var query = _context.Users.AsQueryable();
        int deletedCount = await query.ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return (deletedCount, error: null);
    }

    public async Task<(UserDto? user, string? error)> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return (null, null);
        var dto = user.Adapt<UserDto>();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return (dto, "");
}

    public async Task<(UserDto? user, string? error)> Get(int id)
    {
        throw new NotImplementedException();
    }
}