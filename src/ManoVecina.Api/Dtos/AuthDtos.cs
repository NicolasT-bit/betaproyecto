namespace ManoVecina.Api.Dtos;

public record RegisterRequest(string Email, string Password, string FullName, string? Role, string? AdminKey = null);
public record LoginRequest(string Email, string Password);
public record AuthResponse(int UserId, string Email, string FullName, string Role, string Token);
