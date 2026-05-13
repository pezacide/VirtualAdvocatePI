import { apiGet, apiPatch, apiPost, apiPostNoBody, getAuthHeaders, getApiBaseUrl, handleApiError } from "@/lib/api/client";

export type EvidenceItem = {
  id: string;
  claimWorkspaceId: string;
  conditionId?: string | null;
  evidenceType: string;
  evidenceStatus: string;
  originalFileName?: string | null;
  storagePath?: string | null;
  fileType?: string | null;
  fileSize?: number | null;
  documentDate?: string | null;
  providerName?: string | null;
  userNotes?: string | null;
  aiSummary?: string | null;
  userConfirmedSummary?: string | null;
  usedInGeneratedPack: boolean;
  uploadedAt?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateEvidenceItemInput = {
  evidenceType: string;
  evidenceStatus: string;
  originalFileName?: string;
  storagePath?: string;
  fileType?: string;
  fileSize?: number;
  documentDate?: string;
  providerName?: string;
  userNotes?: string;
  aiSummary?: string;
  userConfirmedSummary?: string;
  usedInGeneratedPack?: boolean;
};


export type UpdateEvidenceItemInput = {
  evidenceType?: string;
  evidenceStatus?: string;
  originalFileName?: string;
  storagePath?: string;
  fileType?: string;
  fileSize?: number;
  documentDate?: string;
  providerName?: string;
  userNotes?: string;
  aiSummary?: string;
  userConfirmedSummary?: string;
  usedInGeneratedPack?: boolean;
};
export type EvidenceUploadUrlResponse = {
  evidenceItem: EvidenceItem;
  upload: {
    method: string;
    url: string;
    expiresInMinutes: number;
    requiredHeaders?: Record<string, string>;
    note?: string;
  };
};

export type CreateEvidenceUploadUrlInput = {
  evidenceType: string;
  originalFileName: string;
  fileType?: string;
  fileSize?: number;
  documentDate?: string;
  providerName?: string;
  userNotes?: string;
};

export function getConditionEvidenceItems(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<EvidenceItem[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-items`,
    "Could not load evidence items.",
  );
}

export function createEvidenceItem(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateEvidenceItemInput,
) {
  return apiPost<EvidenceItem, CreateEvidenceItemInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-items`,
    input,
    "Could not create evidence item.",
  );
}

export function createEvidenceUploadUrl(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateEvidenceUploadUrlInput,
) {
  return apiPost<EvidenceUploadUrlResponse, CreateEvidenceUploadUrlInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-upload-url`,
    input,
    "Could not create evidence upload URL.",
  );
}

export function markEvidenceUploaded(
  idToken: string,
  workspaceId: string,
  evidenceItemId: string,
) {
  return apiPostNoBody<EvidenceItem>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/evidence-items/${evidenceItemId}/mark-uploaded`,
    "Could not confirm evidence upload.",
  );
}

export async function createEvidenceDownloadUrl(
  idToken: string,
  workspaceId: string,
  evidenceItemId: string,
) {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/claim-workspaces/${workspaceId}/evidence-items/${evidenceItemId}/download-url`,
    {
      method: "POST",
      headers: getAuthHeaders(idToken),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not create evidence download URL.");
  }

  return (await response.json()) as {
    evidenceItemId: string;
    method: string;
    url: string;
    expiresInMinutes: number;
  };
}
export function updateEvidenceItem(
  idToken: string,
  workspaceId: string,
  evidenceItemId: string,
  input: UpdateEvidenceItemInput,
) {
  return apiPatch<EvidenceItem, UpdateEvidenceItemInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/evidence-items/${evidenceItemId}`,
    input,
    "Could not update evidence item.",
  );
}

export function updateEvidenceStatus(
  idToken: string,
  workspaceId: string,
  evidenceItemId: string,
  evidenceStatus: string,
) {
  return updateEvidenceItem(idToken, workspaceId, evidenceItemId, {
    evidenceStatus,
  });
}