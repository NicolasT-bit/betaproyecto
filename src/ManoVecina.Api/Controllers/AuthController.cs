using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Data;
using ManoVecina.Api.Domain;
using ManoVecina.Api.Services;
using System.Security.Cryptography;
using System.Text;

namespace ManoVecina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, JwtTokenService jwt, IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
    }

    // ====================================
    // 🔹 REGISTRO DE USUARIO
    // ====================================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        try
        {
            // Validar campos mínimos
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.FullName) ||
                string.IsNullOrWhiteSpace(req.Role))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            // Validar si ya existe un usuario con ese email
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return BadRequest("Ya existe un usuario con ese email.");

            // Validar clave de administrador si el rol es Admin
            if (req.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminKey = _config["Admin:SecretKey"];
                if (req.AdminKey != adminKey)
                    return Unauthorized("Clave de administrador inválida.");

                // Solo permitir un admin en el sistema
                if (await _db.Users.AnyAsync(u => u.Role == UserRole.Admin))
                    return BadRequest("Ya existe un administrador registrado.");
            }

            // ✅ Validar y convertir rol
            if (!Enum.TryParse<UserRole>(req.Role, true, out var parsedRole))
                return BadRequest($"Rol '{req.Role}' no es válido. Usa: 'Customer', 'Worker' o 'Admin'.");

            // ✅ Hash de la contraseña (SHA256)
            using var sha = SHA256.Create();
            var passwordHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(req.Password)));

            // Crear usuario
            var user = new User
            {
                Email = req.Email,
                FullName = req.FullName,
                PasswordHash = passwordHash,
                Role = parsedRole,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Generar token JWT
            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                Role = user.Role.ToString(),
                Token = token
            });
        }
        catch (Exception ex)
        {
            // Captura cualquier error interno (como Enum.Parse o DB)
            return StatusCode(500, new
            {
                error = ex.Message,
                stack = ex.StackTrace
            });
        }
    }

    // ====================================
    // 🔹 LOGIN DE USUARIO
    // ====================================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        // Hash del password ingresado
        using var sha = SHA256.Create();
        var computed = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(req.Password)));

        if (computed != user.PasswordHash)
            return Unauthorized("Contraseña incorrecta.");

        // Generar token JWT
        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            Role = user.Role.ToString(),
            Token = token
        });
    }
}

// ====================================
// 🔸 Modelos para requests
// ====================================
public record RegisterRequest(string Email, string Password, string FullName, string Role, string? AdminKey);
public record LoginRequest(string Email, string Password);
