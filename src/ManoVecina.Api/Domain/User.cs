namespace ManoVecina.Api.Domain;

public enum UserRole
{
  Customer, // Cliente (busca servicios)
  Worker,   // Trabajador (ofrece servicios)
  Admin     // Administrador
}

public class User
{
  public int Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public UserRole Role { get; set; } = UserRole.Customer;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public WorkerProfile? WorkerProfile { get; set; }
}
