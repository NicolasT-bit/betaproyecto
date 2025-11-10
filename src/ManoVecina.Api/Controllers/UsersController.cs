using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Data;
using ManoVecina.Api.Dtos;

namespace ManoVecina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) { _db = db; }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IEnumerable<UserDto>> GetAll()
        => await _db.Users.Select(u => new UserDto(u.Id, u.Email, u.FullName, u.Role.ToString(), u.CreatedAt)).ToListAsync();

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        return new UserDto(u.Id, u.Email, u.FullName, u.Role.ToString(), u.CreatedAt);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto dto)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.FullName = dto.FullName;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        _db.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
