import { apiGet } from "@/lib/api/client";

export type AuditEvent = {
  id: string;
  userId: string;
  claimWorkspaceId?: string | null;
  eventType: string;
  eventDetail?: string | null;
  ipAddress?: string | null;
  clientType?: string | null;
  createdAt: string;
};

export function getWorkspaceAuditEvents(idToken: string, workspaceId: string) {
  return apiGet<AuditEvent[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/audit-events`,
    "Could not load audit events.",
  );
}

export function getWorkspaceAuditEvent(
  idToken: string,
  workspaceId: string,
  auditEventId: string,
) {
  return apiGet<AuditEvent>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/audit-events/${auditEventId}`,
    "Could not load audit event.",
  );
}