namespace ManoVecina.Api.Dtos;

public record ReviewCreateDto(int ServiceRequestId, int Rating, string? Comment);
public record ReviewDto(int Id, int ServiceRequestId, int Rating, string? Comment, DateTime CreatedAt);
