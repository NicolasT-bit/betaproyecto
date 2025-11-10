using Microsoft.EntityFrameworkCore;
using ManoVecina.Api.Domain;

namespace ManoVecina.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 🔹 Tablas principales
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    // ======================================================
    // 🔧 CONFIGURACIÓN EN TIEMPO DE DISEÑO (para migraciones)
    // ======================================================
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Esta conexión se usa SOLO en tiempo de diseño (dotnet ef ...)
            optionsBuilder.UseSqlServer(
                "Server=tcp:proyectofinal-srv.database.windows.net,1433;" +
                "Initial Catalog=ProyectoFinal-db;" +
                "Persist Security Info=False;" +
                "User ID=nicolas;" +
                "Password=Passw0rd;" +
                "MultipleActiveResultSets=False;" +
                "Encrypt=True;" +
                "TrustServerCertificate=False;" +
                "Connection Timeout=30;"
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===========================
        // 🔸 CONFIGURACIONES DE ENTIDADES
        // ===========================

        // Índice único para emails
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Relación 1:1 entre User y WorkerProfile
        modelBuilder.Entity<User>()
            .HasOne(u => u.WorkerProfile)
            .WithOne(w => w.User)
            .HasForeignKey<WorkerProfile>(w => w.UserId);

        // Índice único para nombres de categorías
        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Many-to-Many entre WorkerProfile y ServiceCategory
        modelBuilder.Entity<WorkerProfile>()
            .HasMany(w => w.Categories)
            .WithMany(c => c.Workers)
            .UsingEntity(j => j.ToTable("WorkerCategories"));

        // ===========================
        // 🔸 RELACIONES DE ServiceRequest
        // ===========================

        // Cliente → Cascade (borra solicitudes si se borra el cliente)
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trabajador → Restrict (no borra solicitudes al eliminar trabajador)
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(s => s.Worker)
            .WithMany()
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===========================
        // 🔸 CONFIGURACIONES ADICIONALES
        // ===========================

        modelBuilder.Entity<WorkerProfile>()
            .Property(w => w.HourlyRate)
            .HasPrecision(10, 2);

        // ===========================
        // 🔸 DATOS INICIALES (Seed)
        // ===========================
        modelBuilder.Entity<ServiceCategory>().HasData(
            new ServiceCategory { Id = 1, Name = "Plomería" },
            new ServiceCategory { Id = 2, Name = "Electricidad" },
            new ServiceCategory { Id = 3, Name = "Jardinería" },
            new ServiceCategory { Id = 4, Name = "Pintura" }
        );

        base.OnModelCreating(modelBuilder);
    }
}
