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
        modelBuilder.Entity<UserStatus>().HasIndex(user => user.UserId).IsUnique();

        modelBuilder
            .Entity<UserOauth>()
            .HasIndex(account => new { account.Provider, account.ProviderUid })
            .IsUnique();

        modelBuilder
            .Entity<User>()
            .HasOne(user => user.Status)
            .WithOne()
            .HasForeignKey<UserStatus>(status => status.UserId);
    }
}
