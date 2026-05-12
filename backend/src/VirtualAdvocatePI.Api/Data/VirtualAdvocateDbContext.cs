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

    public DbSet<AcceptedConditionHistory> AcceptedConditionHistories => Set<AcceptedConditionHistory>();

    public DbSet<QuestionResponse> QuestionResponses => Set<QuestionResponse>();

    public DbSet<EvidenceItem> EvidenceItems => Set<EvidenceItem>();

    public DbSet<EvidenceGap> EvidenceGaps => Set<EvidenceGap>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

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
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<AcceptedConditionHistory>(entity =>
        {
            entity.ToTable("accepted_condition_history");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ClaimWorkspaceId);
            entity.HasIndex(x => x.ConditionId);

            entity.Property(x => x.PreviouslyAcceptedByDva).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalAct).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PreviousCompensationReceived).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PreviousDvaDecisionLetterAvailable).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PreviousAssessmentLetterAvailable).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WorseningClaimed).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.ToTable("question_responses");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ClaimWorkspaceId);
            entity.HasIndex(x => x.ConditionId);
            entity.HasIndex(x => new { x.ConditionId, x.QuestionKey });

            entity.Property(x => x.QuestionGroup).HasMaxLength(100).IsRequired();
            entity.Property(x => x.QuestionKey).HasMaxLength(150).IsRequired();
            entity.Property(x => x.QuestionText).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.AnswerType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<EvidenceItem>(entity =>
        {
            entity.ToTable("evidence_items");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ClaimWorkspaceId);
            entity.HasIndex(x => x.ConditionId);
            entity.HasIndex(x => x.EvidenceType);
            entity.HasIndex(x => x.EvidenceStatus);

            entity.Property(x => x.EvidenceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EvidenceStatus).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(500);
            entity.Property(x => x.StoragePath).HasMaxLength(1000);
            entity.Property(x => x.FileType).HasMaxLength(100);
            entity.Property(x => x.ProviderName).HasMaxLength(250);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<EvidenceGap>(entity =>
        {
            entity.ToTable("evidence_gaps");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ClaimWorkspaceId);
            entity.HasIndex(x => x.ConditionId);
            entity.HasIndex(x => x.GapType);
            entity.HasIndex(x => x.GapStatus);
            entity.HasIndex(x => x.Severity);

            entity.Property(x => x.GapType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.GapStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PlainEnglishExplanation).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.SuggestedNextStep).HasMaxLength(1000);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ClaimWorkspaceId);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.EventType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EventDetail).HasMaxLength(2000);
            entity.Property(x => x.IpAddress).HasMaxLength(100);
            entity.Property(x => x.ClientType).HasMaxLength(200);
        });
    }
}
