namespace ManoVecina.Api.Domain;

public class ServiceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Plomería, Electricidad, etc.
    public ICollection<WorkerProfile> Workers { get; set; } = new List<WorkerProfile>();
}
