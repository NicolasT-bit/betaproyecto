using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Data;
using ManoVecina.Api.Dtos;
using ManoVecina.Api.Domain;

namespace ManoVecina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReviewsController(AppDbContext db) { _db = db; }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(ReviewCreateDto dto)
    {
        var sr = await _db.ServiceRequests.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == dto.ServiceRequestId);
        if (sr is null || sr.Status != RequestStatus.Completed) return BadRequest("La solicitud debe estar completada");

        var review = new Review { ServiceRequestId = sr.Id, Rating = dto.Rating, Comment = dto.Comment };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return new ReviewDto(review.Id, review.ServiceRequestId, review.Rating, review.Comment, review.CreatedAt);
    }

    [HttpGet("worker/{workerUserId:int}")]
    public async Task<IEnumerable<ReviewDto>> GetByWorker(int workerUserId)
    {
        var reqIds = await _db.ServiceRequests.Where(s => s.WorkerId == workerUserId).Select(s => s.Id).ToListAsync();
        return await _db.Reviews.Where(r => reqIds.Contains(r.ServiceRequestId))
            .Select(r => new ReviewDto(r.Id, r.ServiceRequestId, r.Rating, r.Comment, r.CreatedAt)).ToListAsync();
    }
}
