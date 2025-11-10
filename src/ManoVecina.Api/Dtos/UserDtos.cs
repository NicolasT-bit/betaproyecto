namespace ManoVecina.Api.Dtos;

public record UserDto(int Id, string Email, string FullName, string Role, DateTime CreatedAt);
