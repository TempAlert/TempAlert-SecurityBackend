using API.Dtos;
using API.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class RolsController : BaseApiController
{
    public IUserService _userService { get; }

    public RolsController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("Rol")]
    public async Task<IActionResult> AddRoleAsync(AddRolDto model)
    {
        var result = await _userService.AddRolAsync(model);
        return Ok(result);
    }
}
