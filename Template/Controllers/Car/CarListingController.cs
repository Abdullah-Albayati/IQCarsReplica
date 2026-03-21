using Template.DTOs;
using Template.Services.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Template.Controllers.Car;

[ApiController]
[Route("api/[controller]")]

public class CarListingController : ControllerBase
{
    private readonly ICarListingService _carListingService;

    public CarListingController(ICarListingService carListingService)
    {
        _carListingService = carListingService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CarListingDto>> GetById(Guid id)
    {
        var result = await _carListingService.GetByIdAsync(id);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpGet("{id}/User")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var car =  await _carListingService.GetByIdAsync(id);
        if (car == null)
            return NotFound();
        
        var user = await _carListingService.GetUserAsync(car.ListingOwnerId);
        if(user == null)
            return NotFound("User not found");
        
        return Ok(new {message = $"User Retrieved: {user.Username} "});
    }
    [HttpGet]
    public async Task<ActionResult<List<CarListingDto>>> GetAll([FromQuery] CarListingFilter listingFilter)
    {
        var (items, totalCount) = await _carListingService.GetAllAsync(listingFilter);
        
        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        
        return Ok(items);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CarListingDto>> Create([FromBody] CarListingForm listingForm)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var listingOwnerId))
        {
            return Unauthorized("Invalid or missing user claim.");
        }

        var result = await _carListingService.CreateAsync(listingForm, listingOwnerId);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CarListingDto>> Update(Guid id, [FromBody] CarListingUpdate listingUpdate)
    {
        var result = await _carListingService.UpdateAsync(id, listingUpdate);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _carListingService.DeleteAsync(id);
        
            return NotFound();
        
        return NoContent();
    }
}
