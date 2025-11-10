using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ManoVecina.Api.Data;
using ManoVecina.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Logging ----------------------
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// ---------------------- DbContext ----------------------
builder.Services.AddDbContext<AppDbContext>();

// ---------------------- CORS ----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvCors", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

// ---------------------- JWT Authentication ----------------------
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// ---------------------- Services ----------------------
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHttpClient<DistanceMatrixService>();

// ---------------------- Controllers ----------------------
builder.Services.AddControllers();

// ---------------------- Swagger (con JWT Authorize) ----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ManoVecina API", Version = "v1" });

    // 🔐 Agregar soporte para el botón Authorize (Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo **Bearer**. Ejemplo: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ---------------------- Middleware ----------------------
app.UseSerilogRequestLogging();

// Swagger siempre activo (dev o prod)
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("MvCors");

app.UseAuthentication(); // 🔒 Primero autenticación
app.UseAuthorization();  // 🔑 Luego autorización

app.MapControllers();
app.Run();
