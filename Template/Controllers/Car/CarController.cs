using Template.DTOs;
using Template.Services.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Template.Controllers.Car;

[ApiController]
[Route("api/[controller]")]

public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    public CarController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CarDto>> GetById(Guid id)
    {
        var result = await _carService.GetByIdAsync(id);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpGet("{id}/User")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var car =  await _carService.GetByIdAsync(id);
        if (car == null)
            return NotFound();
        
        var user = await _carService.GetUserAsync(car.ListingOwnerId);
        if(user == null)
            return NotFound("User not found");
        
        return Ok(new {message = $"User Retrieved: {user.Username} "});
    }
    [HttpGet]
    public async Task<ActionResult<List<CarDto>>> GetAll([FromQuery] CarFilter filter)
    {
        var (items, totalCount) = await _carService.GetAllAsync(filter);
        
        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        
        return Ok(items);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CarDto>> Create([FromBody] CarForm form)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var listingOwnerId))
        {
            return Unauthorized("Invalid or missing user claim.");
        }

        var result = await _carService.CreateAsync(form, listingOwnerId);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CarDto>> Update(Guid id, [FromBody] CarUpdate update)
    {
        var result = await _carService.UpdateAsync(id, update);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _carService.DeleteAsync(id);
        
            return NotFound();
        
        return NoContent();
    }
}
