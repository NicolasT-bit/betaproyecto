using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Data;
using ManoVecina.Api.Domain;

namespace ManoVecina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServiceCategoriesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IEnumerable<ServiceCategory>> Get() => await _db.ServiceCategories.AsNoTracking().ToListAsync();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ServiceCategory>> Create(ServiceCategory c)
    {
        _db.ServiceCategories.Add(c); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }
}
