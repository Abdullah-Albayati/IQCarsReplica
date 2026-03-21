using Template.DTOs;
using Template.Entities.CarEntity;
using Template.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Template.Services.Car;

public interface ICarListingService
{
    Task<CarListingDto?> GetByIdAsync(Guid id);
    Task<(List<CarListingDto> items, int totalCount)> GetAllAsync(CarListingFilter listingFilter);
    Task<UserDto> GetUserAsync(int id);
    Task<CarListingDto> CreateAsync(CarListingForm listingForm, int listingOwnerId);
    Task<CarListingDto?> UpdateAsync(Guid id, CarListingUpdate listingUpdate);
    Task<bool> DeleteAsync(Guid id);
}

public class CarListingService : ICarListingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CarListingService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CarListingDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Cars.FindAsync(id);
        return entity == null ? null : _mapper.Map<CarListingDto>(entity);
    }

    public async Task<(List<CarListingDto> items, int totalCount)> GetAllAsync(CarListingFilter listingFilter)
    {
        var query = _context.Cars.AsQueryable();
        
        // TODO: Apply filters
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((listingFilter.PageNumber - 1) * listingFilter.PageSize)
            .Take(listingFilter.PageSize)
            .ToListAsync();
        
        return (_mapper.Map<List<CarListingDto>>(items), totalCount);
    }

    public async Task<CarListingDto> CreateAsync(CarListingForm listingForm, int listingOwnerId)
    {
        var entity = _mapper.Map<Template.Entities.CarEntity.CarListing>(listingForm);
        entity.ListingOwnerId = listingOwnerId;
        
        _context.Cars.Add(entity);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CarListingDto>(entity);
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return _mapper.Map<UserDto>(user);
    }
    public async Task<CarListingDto?> UpdateAsync(Guid id, CarListingUpdate listingUpdate)
    {
        var entity = await _context.Cars.FindAsync(id);
        
        if (entity == null)
            return null;
        
        _mapper.Map(listingUpdate, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CarListingDto>(entity);
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
