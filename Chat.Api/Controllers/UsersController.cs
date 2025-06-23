using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> Get(
        [FromQuery] Guid? id,
        [FromQuery] string? username,
        [FromQuery] string? role,
        [FromQuery] bool? active)
    {
        var users = await _userRepository.GetAllAsync();

        if (id.HasValue)
            users = users.Where(u => u.Id == id.Value);

        if (!string.IsNullOrWhiteSpace(username))
            users = users.Where(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(role))
            users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

        if (active.HasValue)
            users = users.Where(u => u.Active == active.Value);

        var response = users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            Active = u.Active
        });

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromQuery] Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Role = request.Role;
        user.Active = request.Active;

        await _userRepository.UpdateAsync(user);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromQuery] Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userRepository.DeleteAsync(user);
        return NoContent();
    }
}
