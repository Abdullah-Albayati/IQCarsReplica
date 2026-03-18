using Microsoft.AspNetCore.Mvc;
using Template.DTOs;
using Template.Services;

namespace Template.Controllers.User;

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
    public async Task<ActionResult<object>> GetAll([FromQuery] UserFilter filter)
    {
        var (users, totalCount, error) = await _userServices.GetAll(filter);
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(error);
        }

        return Ok(new { users, totalCount });
    }

    [HttpDelete("delete-all")]
    public async Task<ActionResult<object>> DeleteAll()
    {
        var (count, _) = await _userServices.DeleteAll();
        return Ok(new { message = $"Deleted {count} users" });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserUpdate userUpdate)
    {
        var (user, error) = await _userServices.Update(id, userUpdate);
        if (user == null)
        {
            return BadRequest(error);
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<UserDto>> Delete(int id)
    {
        var (user, error) = await _userServices.Delete(id);
        if (user == null)
        {
            return NotFound(error);
        }

        return Ok(user);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        var (user, error) = await _userServices.Get(id);
        if (user == null)
        {
            return NotFound(error);
        }

        return Ok(user);
    }
}

