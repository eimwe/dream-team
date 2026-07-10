using dream_team.Models;
using Microsoft.EntityFrameworkCore;

namespace dream_team.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public required DbSet<User> Users { get; set; }
    public required DbSet<UserRole> UserRoles { get; set; }
    public required DbSet<UserOauth> UserOauths { get; set; }
    public required DbSet<UserStatus> UserStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserStatus>().HasIndex(u => u.UserId).IsUnique();

        modelBuilder
            .Entity<UserOauth>()
            .HasIndex(u => new { u.Provider, u.ProviderUid })
            .IsUnique();
    }
}
