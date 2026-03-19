using Template.DTOs;
using Template.Entities.CarEntity;
using Template.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Template.Services.Car;

public interface ICarService
{
    Task<CarDto?> GetByIdAsync(Guid id);
    Task<(List<CarDto> items, int totalCount)> GetAllAsync(CarFilter filter);
    Task<UserDto> GetUserAsync(int id);
    Task<CarDto> CreateAsync(CarForm form, int listingOwnerId);
    Task<CarDto?> UpdateAsync(Guid id, CarUpdate update);
    Task<bool> DeleteAsync(Guid id);
}

public class CarService : ICarService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CarService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CarDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Cars.FindAsync(id);
        return entity == null ? null : _mapper.Map<CarDto>(entity);
    }

    public async Task<(List<CarDto> items, int totalCount)> GetAllAsync(CarFilter filter)
    {
        var query = _context.Cars.AsQueryable();
        
        // TODO: Apply filters
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        
        return (_mapper.Map<List<CarDto>>(items), totalCount);
    }

    public async Task<CarDto> CreateAsync(CarForm form, int listingOwnerId)
    {
        var entity = _mapper.Map<Template.Entities.CarEntity.Car>(form);
        entity.ListingOwnerId = listingOwnerId;
        
        _context.Cars.Add(entity);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CarDto>(entity);
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return _mapper.Map<UserDto>(user);
    }
    public async Task<CarDto?> UpdateAsync(Guid id, CarUpdate update)
    {
        var entity = await _context.Cars.FindAsync(id);
        
        if (entity == null)
            return null;
        
        _mapper.Map(update, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CarDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Cars.FindAsync(id);
        
        if (entity == null)
            return false;
        
        _context.Cars.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }
}
