namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid? ClaimWorkspaceId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string? EventDetail { get; set; }

    public string? IpAddress { get; set; }

    public string? ClientType { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
