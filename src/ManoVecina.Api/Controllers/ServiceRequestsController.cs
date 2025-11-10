using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Data;
using ManoVecina.Api.Domain;
using ManoVecina.Api.Dtos;
using ManoVecina.Api.Services;

namespace ManoVecina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly DistanceMatrixService _dm;
    public ServiceRequestsController(AppDbContext db, DistanceMatrixService dm) { _db = db; _dm = dm; }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> Create(ServiceRequestCreateDto dto)
    {
        var userId = int.Parse(User.Claims.First(c => c.Type.EndsWith("/nameidentifier") || c.Type.Contains("sub")).Value);
        var worker = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.WorkerUserId);
        if (worker is null || worker.Role != UserRole.Worker) return BadRequest("Trabajador inválido");

        var sr = new ServiceRequest
        {
            CustomerId = userId,
            WorkerId = worker.Id,
            Description = dto.Description,
            CustomerLat = dto.CustomerLat,
            CustomerLng = dto.CustomerLng
        };

        // Distancia con Google Distance Matrix si existe API Key
        var wprof = await _db.WorkerProfiles.FirstOrDefaultAsync(w => w.UserId == worker.Id);
        if (wprof != null)
        {
            sr.DistanceKm = await _dm.GetDistanceKmAsync(dto.CustomerLat, dto.CustomerLng, wprof.Latitude, wprof.Longitude);
        }

        _db.ServiceRequests.Add(sr);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = sr.Id }, ToDto(sr));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestDto>> GetById(int id)
    {
        var sr = await _db.ServiceRequests.FindAsync(id);
        if (sr is null) return NotFound();
        return ToDto(sr);
    }

    [Authorize(Roles = "Worker,Admin")]
    [HttpPut("{id:int}/status")] 
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] RequestStatus status)
    {
        var sr = await _db.ServiceRequests.FindAsync(id);
        if (sr is null) return NotFound();
        sr.Status = status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ServiceRequestDto ToDto(ServiceRequest s) => new(
        s.Id, s.CustomerId, s.WorkerId, s.Description, s.RequestedAt, s.Status.ToString(), s.CustomerLat, s.CustomerLng, s.DistanceKm);
}
