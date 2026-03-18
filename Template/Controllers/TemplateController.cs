using Microsoft.AspNetCore.Mvc;

namespace Template.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok", timestampUtc = DateTime.UtcNow });
    }
}

