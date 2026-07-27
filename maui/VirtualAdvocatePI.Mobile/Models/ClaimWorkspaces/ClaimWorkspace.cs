namespace VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;

public sealed class ClaimWorkspace
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string ClaimFramework { get; init; } = string.Empty;

    public string ClaimScenario { get; init; } = string.Empty;

    public string WorkspaceTitle { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string GeneratedPackStatus { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? LastOpenedAt { get; init; }
}
