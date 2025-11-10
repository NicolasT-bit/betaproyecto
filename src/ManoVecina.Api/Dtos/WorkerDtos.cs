namespace ManoVecina.Api.Dtos;

public record WorkerCreateDto(string Bio, double Latitude, double Longitude, bool IsAvailable, decimal? HourlyRate, int[] CategoryIds);
public record WorkerDto(int Id, int UserId, string FullName, string Bio, double Latitude, double Longitude, bool IsAvailable, decimal? HourlyRate, double? AverageRating, int[] Categories);
