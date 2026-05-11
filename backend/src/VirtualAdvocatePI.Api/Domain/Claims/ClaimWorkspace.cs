namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class ClaimWorkspace
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string ClaimFramework { get; set; } = "IMPROVED_MRCA_POST_2026";

    public string ClaimScenario { get; set; } = "UNSURE";

    public string WorkspaceTitle { get; set; } = "Post-2026 PI Claim Starter Pack";

    public string Status { get; set; } = "NOT_STARTED";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastOpenedAt { get; set; }

    public string GeneratedPackStatus { get; set; } = "NOT_GENERATED";
}
