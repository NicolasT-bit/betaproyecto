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
public class WorkersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DistanceMatrixService _dm;
    public WorkersController(AppDbContext db, DistanceMatrixService dm) { _db = db; _dm = dm; }

    [HttpGet]
    public async Task<IEnumerable<WorkerDto>> GetAll()
    {
        var workers = await _db.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Categories)
            .Include(w => w.Reviews)
            .ToListAsync();

        return workers.Select(w => new WorkerDto(
            w.Id,
            w.UserId,
            w.User.FullName,
            w.Bio,
            w.Latitude,
            w.Longitude,
            w.IsAvailable,
            w.HourlyRate,
            w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : null,
            w.Categories.Select(c => c.Id).ToArray()));
    }

    [Authorize(Roles = "Worker,Admin")]
    [HttpPost]
    public async Task<ActionResult<WorkerDto>> Create([FromBody] WorkerCreateDto dto)
    {
        var userId = int.Parse(User.Claims.First(c => c.Type.EndsWith("/nameidentifier") || c.Type.Contains("sub")).Value);
        var profile = new WorkerProfile
        {
            UserId = userId,
            Bio = dto.Bio,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsAvailable = dto.IsAvailable,
            HourlyRate = dto.HourlyRate
        };
        profile.Categories = await _db.ServiceCategories.Where(c => dto.CategoryIds.Contains(c.Id)).ToListAsync();
        _db.WorkerProfiles.Add(profile);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, new WorkerDto(
            profile.Id, profile.UserId, (await _db.Users.FindAsync(profile.UserId))!.FullName, profile.Bio,
            profile.Latitude, profile.Longitude, profile.IsAvailable, profile.HourlyRate, null,
            profile.Categories.Select(c => c.Id).ToArray()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkerDto>> GetById(int id)
    {
        var w = await _db.WorkerProfiles.Include(x => x.User).Include(x => x.Categories).Include(x => x.Reviews).FirstOrDefaultAsync(x => x.Id == id);
        if (w is null) return NotFound();
        return new WorkerDto(w.Id, w.UserId, w.User.FullName, w.Bio, w.Latitude, w.Longitude, w.IsAvailable, w.HourlyRate,
            w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : null,
            w.Categories.Select(c => c.Id).ToArray());
    }

    [HttpGet("nearby")]
    public async Task<IEnumerable<WorkerDto>> Nearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 5)
    {
        var workers = await _db.WorkerProfiles.Include(w => w.User).Include(w => w.Categories).Include(w => w.Reviews)
            .Where(w => w.IsAvailable)
            .ToListAsync();

        // Haversine
        static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // km
            var dLat = Math.PI / 180 * (lat2 - lat1);
            var dLon = Math.PI / 180 * (lon2 - lon1);
            var a = Math.Sin(dLat/2)*Math.Sin(dLat/2) + Math.Cos(Math.PI/180*lat1)*Math.Cos(Math.PI/180*lat2)*Math.Sin(dLon/2)*Math.Sin(dLon/2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            return R * c;
        }

        var result = new List<WorkerDto>();
        foreach (var w in workers)
        {
            var dist = Haversine(lat, lng, w.Latitude, w.Longitude);
            if (dist <= radiusKm)
            {
                result.Add(new WorkerDto(w.Id, w.UserId, w.User.FullName, w.Bio, w.Latitude, w.Longitude, w.IsAvailable, w.HourlyRate,
                    w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : null,
                    w.Categories.Select(c => c.Id).ToArray()));
            }
        }
        return result.OrderBy(x => x.AverageRating ?? double.MaxValue);
    }
}
