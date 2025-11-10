namespace ManoVecina.Api.Dtos;

public record ServiceRequestCreateDto(int WorkerUserId, string Description, double CustomerLat, double CustomerLng);
public record ServiceRequestDto(int Id, int CustomerId, int WorkerId, string Description, DateTime RequestedAt, string Status, double CustomerLat, double CustomerLng, double? DistanceKm);
