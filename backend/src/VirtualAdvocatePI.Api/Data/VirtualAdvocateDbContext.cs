using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Admin;
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

    public DbSet<AiSourceRegistryEntry> AiSourceRegistryEntries => Set<AiSourceRegistryEntry>();

    public DbSet<AdminTemplateRegistryEntry> AdminTemplateRegistryEntries => Set<AdminTemplateRegistryEntry>();

    public DbSet<AdminPromptDisclaimerVersionEntry> AdminPromptDisclaimerVersionEntries => Set<AdminPromptDisclaimerVersionEntry>();

    public DbSet<AiDraft> AiDrafts => Set<AiDraft>();

    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();

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

        modelBuilder.Entity<AiSourceRegistryEntry>(entity =>
        {
            entity.ToTable("ai_source_registry_entries");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.SourceKey).IsUnique();
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.SourceType);
            entity.HasIndex(x => x.ApprovalStatus);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsActive);

            entity.Property(x => x.SourceKey).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Jurisdiction).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SourceVersion).HasMaxLength(100);
            entity.Property(x => x.CitationLabel).HasMaxLength(300).IsRequired();
            entity.Property(x => x.SourceUrl).HasMaxLength(2000);
            entity.Property(x => x.StoragePath).HasMaxLength(1000);
            entity.Property(x => x.ContentHash).HasMaxLength(200);
            entity.Property(x => x.ApprovalStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ApprovedBy).HasMaxLength(200);
            entity.Property(x => x.ReviewNotes).HasMaxLength(2000);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

modelBuilder.Entity<AiDraft>(entity =>
{
    entity.ToTable("ai_drafts");

    entity.HasKey(x => x.Id);

    entity.HasIndex(x => x.ClaimWorkspaceId);
    entity.HasIndex(x => x.ConditionId);
    entity.HasIndex(x => x.DraftType);
    entity.HasIndex(x => x.ReviewStatus);

    entity.Property(x => x.DraftType).HasMaxLength(150).IsRequired();
    entity.Property(x => x.PromptVersion).HasMaxLength(150).IsRequired();
    entity.Property(x => x.SourceReferences).HasMaxLength(4000);
    entity.Property(x => x.DraftText).IsRequired();
    entity.Property(x => x.UserEditedText);
    entity.Property(x => x.ReviewStatus).HasMaxLength(100).IsRequired();
    entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
});

modelBuilder.Entity<GeneratedDocument>(entity =>
{
    entity.ToTable("generated_documents");

    entity.HasKey(x => x.Id);

    entity.HasIndex(x => x.ClaimWorkspaceId);
    entity.HasIndex(x => x.DocumentType);
    entity.HasIndex(x => x.DocumentStatus);

    entity.Property(x => x.DocumentType).HasMaxLength(150).IsRequired();
    entity.Property(x => x.DocumentStatus).HasMaxLength(100).IsRequired();
    entity.Property(x => x.DocxStoragePath).HasMaxLength(1000);
    entity.Property(x => x.PdfStoragePath).HasMaxLength(1000);
    entity.Property(x => x.TemplateVersion).HasMaxLength(150).IsRequired();
    entity.Property(x => x.IncludedAiDraftIds).HasMaxLength(4000);
    entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
});
        modelBuilder.Entity<AdminPromptDisclaimerVersionEntry>(entity =>
        {
            entity.ToTable("admin_prompt_disclaimer_version_entries");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.VersionKey).IsUnique();
            entity.HasIndex(x => x.VersionType);
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.AppliesTo);
            entity.HasIndex(x => x.ApprovalStatus);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.Status);

            entity.Property(x => x.VersionKey).HasMaxLength(150).IsRequired();
            entity.Property(x => x.VersionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.VersionLabel).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AppliesTo).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.ApprovalStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ApprovedBy).HasMaxLength(250);
            entity.Property(x => x.ReviewNotes).HasMaxLength(4000);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });


        modelBuilder.Entity<AdminTemplateRegistryEntry>(entity =>
        {
            entity.ToTable("admin_template_registry_entries");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateKey)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.TemplateType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.TemplateVersion)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.TemplateBody)
                .IsRequired();

            entity.Property(x => x.OutputFormat)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.ApprovalStatus)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.ApprovedBy)
                .HasMaxLength(250);

            entity.Property(x => x.ReviewNotes)
                .HasMaxLength(4000);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.TemplateKey)
                .IsUnique();

            entity.HasIndex(x => x.TemplateType);

            entity.HasIndex(x => x.Category);

            entity.HasIndex(x => x.ApprovalStatus);

            entity.HasIndex(x => x.IsActive);

            entity.HasIndex(x => x.Status);
        });
    }
}
