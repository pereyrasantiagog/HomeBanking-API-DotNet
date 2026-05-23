using HomeBankingBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeBankingBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Regla de seguridad: Evitar que al borrar una cuenta, se borren las transacciones en cascada
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.SourceAccount)
            .WithMany()
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.DestinationAccount)
            .WithMany()
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Data Seeding: Usuarios de prueba
        modelBuilder.Entity<User>().HasData(
            new User { Id = 101, FirstName = "Santiago", LastName = "Perez", Email = "santiago@ejemplo.com", Password = BCrypt.Net.BCrypt.HashPassword("1234") },
            new User { Id = 102, FirstName = "Maria", LastName = "Gomez", Email = "maria@ejemplo.com", Password = BCrypt.Net.BCrypt.HashPassword("5678") }
        );

        // Data Seeding: Cuentas bancarias
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 101, Number = "VIN-00000101", CreationDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), Balance = 150000, UserId = 101 },
            new Account { Id = 102, Number = "VIN-00000102", CreationDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), Balance = 50000, UserId = 102 }
        );
    }
}
