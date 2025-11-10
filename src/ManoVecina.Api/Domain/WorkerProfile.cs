namespace ManoVecina.Api.Domain;

public class WorkerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public string Bio { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal? HourlyRate { get; set; }

    public ICollection<ServiceCategory> Categories { get; set; } = new List<ServiceCategory>();
    public ICollection<MediaAsset> Portfolio { get; set; } = new List<MediaAsset>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
