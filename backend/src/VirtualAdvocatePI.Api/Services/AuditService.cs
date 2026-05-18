using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;

namespace VirtualAdvocatePI.Api.Services;

public sealed class AuditService
{
    private readonly VirtualAdvocateDbContext _db;

    public AuditService(VirtualAdvocateDbContext db)
    {
        _db = db;
    }

    public void AddAuditEvent(
        HttpRequest request,
        Guid userId,
        Guid workspaceId,
        string eventType,
        string? eventDetail)
    {
        request.Headers.TryGetValue("User-Agent", out var userAgent);

        _db.AuditEvents.Add(new AuditEvent
        {
            UserId = userId,
            ClaimWorkspaceId = workspaceId,
            EventType = eventType,
            EventDetail = eventDetail,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            ClientType = userAgent.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public void AddAdminAuditEvent(
        HttpRequest request,
        Guid userId,
        string eventType,
        string? eventDetail)
    {
        AddAuditEvent(
            request,
            userId,
            Guid.Empty,
            eventType,
            eventDetail);
    }
}