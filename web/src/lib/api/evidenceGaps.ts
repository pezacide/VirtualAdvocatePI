import { apiGet, apiPatch, apiPostNoBody } from "@/lib/api/client";

export type EvidenceGap = {
  id: string;
  claimWorkspaceId: string;
  conditionId: string;
  gapType: string;
  gapStatus: string;
  severity: string;
  plainEnglishExplanation: string;
  suggestedNextStep?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type RecalculateEvidenceGapsResponse = {
  conditionId: string;
  createdCount: number;
  gaps: EvidenceGap[];
};

export type UpdateEvidenceGapInput = {
  gapStatus?: string;
  severity?: string;
  plainEnglishExplanation?: string;
  suggestedNextStep?: string;
};

export function getWorkspaceEvidenceGaps(idToken: string, workspaceId: string) {
  return apiGet<EvidenceGap[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/evidence-gaps`,
    "Could not load evidence gaps.",
  );
}

export function getConditionEvidenceGaps(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<EvidenceGap[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps`,
    "Could not load condition evidence gaps.",
  );
}

export function recalculateEvidenceGaps(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiPostNoBody<RecalculateEvidenceGapsResponse>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps/recalculate`,
    "Could not recalculate evidence gaps.",
  );
}

export function updateEvidenceGap(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  gapId: string,
  input: UpdateEvidenceGapInput,
) {
  return apiPatch<EvidenceGap, UpdateEvidenceGapInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps/${gapId}`,
    input,
    "Could not update evidence gap.",
  );
}