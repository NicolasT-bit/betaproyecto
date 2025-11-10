namespace ManoVecina.Api.Domain;

public class MediaAsset
{
    public int Id { get; set; }
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = default!;

    public string Url { get; set; } = string.Empty; // Cloud Storage / CDN
    public string? Caption { get; set; }
}
