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
  documentType: string;
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