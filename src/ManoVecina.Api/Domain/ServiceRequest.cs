namespace ManoVecina.Api.Domain;

public enum RequestStatus { Pending, Accepted, Rejected, Completed }

public class ServiceRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public User Customer { get; set; } = default!;

    public int WorkerId { get; set; }
    public User Worker { get; set; } = default!;

    public string Description { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public double CustomerLat { get; set; }
    public double CustomerLng { get; set; }
    public double? DistanceKm { get; set; } // desde Google Distance Matrix
}
