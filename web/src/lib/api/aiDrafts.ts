import {
  apiGet,
  apiPatch,
  apiPost,
  getAuthHeaders,
  getApiBaseUrl,
  handleApiError,
} from "@/lib/api/client";

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
  promptVersion: string;
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

export type GenerateAiDraftInput = {
  conditionId: string;
  draftType: string;
  query?: string;
  maxSources?: number;
  userInstruction?: string;
};

export type GenerateAiDraftResponse = {
  aiDraft: AiDraft;
  sourceReferences: Array<{
    citationMarker: string;
    sourceKey: string;
    citationLabel: string;
    category: string;
    sourceType: string;
    chunkKey: string;
    chunkTitle: string;
  }>;
  safety: {
    preparationSupportOnly: boolean;
    requiresUserReview: boolean;
    legalAdvice: boolean;
    medicalAdvice: boolean;
    diagnosis: boolean;
    dvaDecision: boolean;
    impairmentCalculation: boolean;
    compensationEstimate: boolean;
    outcomeGuarantee: boolean;
    aiModelCalled: boolean;
  };
};

export type ArchiveAiDraftResponse = {
  id: string;
  status: string;
  archived: boolean;
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

export function generateAiDraft(
  idToken: string,
  workspaceId: string,
  input: GenerateAiDraftInput,
) {
  return apiPost<GenerateAiDraftResponse, GenerateAiDraftInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/ai-drafts/generate`,
    input,
    "Could not generate AI draft.",
  );
}

export async function archiveAiDraft(
  idToken: string,
  workspaceId: string,
  draftId: string,
) {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/claim-workspaces/${workspaceId}/ai-drafts/${draftId}`,
    {
      method: "DELETE",
      headers: getAuthHeaders(idToken),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not archive AI draft.");
  }

  return (await response.json()) as ArchiveAiDraftResponse;
}