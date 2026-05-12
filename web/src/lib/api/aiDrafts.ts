import { apiGet, apiPatch, apiPost } from "@/lib/api/client";

export type AiDraft = {
  id: string;
  claimWorkspaceId: string;
  conditionId?: string | null;
  draftType: string;
  promptVersion: string;
  sourceReferences?: string | null;
  draftText: string;
  userEditedText?: string | null;
  reviewStatus: string;
  approvedAt?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateAiDraftInput = {
  conditionId?: string;
  draftType: string;
  promptVersion?: string;
  sourceReferences?: string;
  draftText: string;
  userEditedText?: string;
  reviewStatus?: string;
};

export type UpdateAiDraftInput = {
  draftType?: string;
  promptVersion?: string;
  sourceReferences?: string;
  draftText?: string;
  userEditedText?: string;
  reviewStatus?: string;
};

export function getWorkspaceAiDrafts(idToken: string, workspaceId: string) {
  return apiGet<AiDraft[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/ai-drafts`,
    "Could not load AI drafts.",
  );
}

export function getConditionAiDrafts(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<AiDraft[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/ai-drafts`,
    "Could not load condition AI drafts.",
  );
}

export function createAiDraft(
  idToken: string,
  workspaceId: string,
  input: CreateAiDraftInput,
) {
  return apiPost<AiDraft, CreateAiDraftInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/ai-drafts`,
    input,
    "Could not create AI draft.",
  );
}

export function updateAiDraft(
  idToken: string,
  workspaceId: string,
  draftId: string,
  input: UpdateAiDraftInput,
) {
  return apiPatch<AiDraft, UpdateAiDraftInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/ai-drafts/${draftId}`,
    input,
    "Could not update AI draft.",
  );
}