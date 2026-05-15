import { apiGet, apiPatch, apiPost } from "@/lib/api/client";

export type GeneratedDocument = {
  id: string;
  claimWorkspaceId: string;
  documentType: string;
  documentStatus: string;
  docxStoragePath?: string | null;
  pdfStoragePath?: string | null;
  templateVersion: string;
  includedAiDraftIds?: string | null;
  generatedAt?: string | null;
  downloadedAt?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateGeneratedDocumentInput = {
  documentType?: string;
  documentStatus?: string;
  docxStoragePath?: string;
  pdfStoragePath?: string;
  templateVersion?: string;
  includedAiDraftIds?: string;
};

export type UpdateGeneratedDocumentInput = {
  documentType?: string;
  documentStatus?: string;
  docxStoragePath?: string;
  pdfStoragePath?: string;
  templateVersion?: string;
  includedAiDraftIds?: string;
};

export type GenerateClaimStarterPackResponse = {
  document: GeneratedDocument;
  generated: boolean;
  docxStoragePath: string;
  pdfStoragePath?: string | null;
  documentVersion?: string;
  includedAiDraftCount: number;
  activeConditionCount: number;
  evidenceItemCount: number;
  evidenceGapCount: number;
};

export type GeneratedDocumentDownloadUrlResponse = {
  documentId: string;
  format: "DOCX" | "PDF";
  url: string;
  method: "GET";
  expiresInMinutes: number;
  storagePath: string;
  document: GeneratedDocument;
};

export function getGeneratedDocuments(idToken: string, workspaceId: string) {
  return apiGet<GeneratedDocument[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/generated-documents`,
    "Could not load generated documents.",
  );
}

export function createGeneratedDocument(
  idToken: string,
  workspaceId: string,
  input: CreateGeneratedDocumentInput,
) {
  return apiPost<GeneratedDocument, CreateGeneratedDocumentInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/generated-documents`,
    input,
    "Could not create generated document metadata.",
  );
}

export function updateGeneratedDocument(
  idToken: string,
  workspaceId: string,
  documentId: string,
  input: UpdateGeneratedDocumentInput,
) {
  return apiPatch<GeneratedDocument, UpdateGeneratedDocumentInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/generated-documents/${documentId}`,
    input,
    "Could not update generated document metadata.",
  );
}

export function generateClaimStarterPack(
  idToken: string,
  workspaceId: string,
) {
  return apiPost<GenerateClaimStarterPackResponse, { notes?: string }>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/generated-documents/claim-starter-pack`,
    {},
    "Could not generate Claim Starter Pack.",
  );
}

export function createGeneratedDocumentDownloadUrl(
  idToken: string,
  workspaceId: string,
  documentId: string,
  format: "DOCX" | "PDF",
) {
  return apiPost<GeneratedDocumentDownloadUrlResponse, { format: "DOCX" | "PDF" }>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/generated-documents/${documentId}/download-url`,
    { format },
    `Could not create ${format} download link.`,
  );
}