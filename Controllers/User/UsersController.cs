using HotelSystemBackend.DTOs;
using HotelSystemBackend.Entities;
using HotelSystemBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystemBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserServices _userServices;

    public UsersController(IUserServices userServices)
    {
        _userServices = userServices;
    }

  
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] UserFilter filter)
    {
        var (users, totalCount, error) = await _userServices.GetAll(filter);
        if (!string.IsNullOrEmpty(error))
            return BadRequest(error);

        return Ok(new { users, totalCount, error });
    }

    [HttpDelete("Delete-All")]
    public async Task<ActionResult> DeleteAll()
    {
        var count = await _userServices.DeleteAll();
        return Ok(new { Message = $"Deleted {count} users" });
    }

    [HttpPut("{id}")]
    
    public async Task<ActionResult<User>> Update(int id, [FromQuery] UserUpdate userUpdate)
    {
        var (user, error) = await _userServices.Update(id, userUpdate);
        if(user == null)
            return BadRequest(error);
        return Ok(user);
    }

[HttpDelete("{id}")]
    public async Task<ActionResult<User>> Delete(int id)
    {
        var user = await _userServices.Delete(id);
        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(int id) => Ok(await _userServices.Get(id));
}
