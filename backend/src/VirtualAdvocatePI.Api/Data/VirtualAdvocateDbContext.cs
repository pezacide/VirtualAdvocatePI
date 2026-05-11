using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Users;

namespace VirtualAdvocatePI.Api.Data;

public sealed class VirtualAdvocateDbContext : DbContext
{
    public VirtualAdvocateDbContext(DbContextOptions<VirtualAdvocateDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<ClaimWorkspace> ClaimWorkspaces => Set<ClaimWorkspace>();

    public DbSet<ClaimCondition> ClaimConditions => Set<ClaimCondition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.FirebaseUid).IsUnique();

            entity.Property(x => x.FirebaseUid).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AccountStatus).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<ClaimWorkspace>(entity =>
        {
            entity.ToTable("claim_workspaces");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId);

            entity.Property(x => x.ClaimFramework).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ClaimScenario).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WorkspaceTitle).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(100).IsRequired();
            entity.Property(x => x.GeneratedPackStatus).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ClaimCondition>(entity =>
        {
            entity.ToTable("claim_conditions");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ClaimWorkspaceId);

            entity.Property(x => x.ConditionName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.DiagnosisStatus).HasMaxLength(100).IsRequired();
        });
    }
}
