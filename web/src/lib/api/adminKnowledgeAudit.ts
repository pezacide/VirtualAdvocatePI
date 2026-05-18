import { apiGet } from "@/lib/api/client";

export type AdminKnowledgeAuditEvent = {
  id: string;
  userId: string;
  claimWorkspaceId: string;
  eventType: string;
  eventDetail?: string | null;
  ipAddress?: string | null;
  clientType?: string | null;
  createdAt: string;
};

export type AdminKnowledgeAuditEventTypeSummary = {
  eventType: string;
  count: number;
};

export type AdminKnowledgeAuditResponse = {
  totalReturned: number;
  eventTypeSummary: AdminKnowledgeAuditEventTypeSummary[];
  rows: AdminKnowledgeAuditEvent[];
};

export type AdminKnowledgeAuditFilters = {
  search?: string;
  eventType?: string;
  workspaceId?: string;
  userId?: string;
  from?: string;
  to?: string;
  knowledgeOnly?: string;
};

export function getAdminKnowledgeAuditEvents(
  idToken: string,
  filters: AdminKnowledgeAuditFilters = {},
) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();

  return apiGet<AdminKnowledgeAuditResponse>(
    idToken,
    `/api/v1/admin/knowledge-audit${query ? `?${query}` : ""}`,
    "Could not load knowledge base audit events.",
  );
}

export function getAdminKnowledgeAuditEvent(idToken: string, auditEventId: string) {
  return apiGet<AdminKnowledgeAuditEvent>(
    idToken,
    `/api/v1/admin/knowledge-audit/${auditEventId}`,
    "Could not load knowledge base audit event.",
  );
}