import { apiGet, apiPost } from "@/lib/api/client";

export type AcceptedConditionHistory = {
  id: string;
  claimWorkspaceId: string;
  conditionId: string;
  previouslyAcceptedByDva: string;
  originalAct: string;
  previousCompensationReceived: string;
  previousDvaDecisionLetterAvailable: string;
  previousAssessmentLetterAvailable: string;
  previousDecisionDate?: string | null;
  previousAssessmentDate?: string | null;
  worseningClaimed: string;
  worseningSummary?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateAcceptedConditionHistoryInput = {
  previouslyAcceptedByDva: string;
  originalAct: string;
  previousCompensationReceived: string;
  previousDvaDecisionLetterAvailable: string;
  previousAssessmentLetterAvailable: string;
  previousDecisionDate?: string;
  previousAssessmentDate?: string;
  worseningClaimed: string;
  worseningSummary?: string;
};

export function getAcceptedConditionHistory(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<AcceptedConditionHistory[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/accepted-history`,
    "Could not load accepted-condition history.",
  );
}

export function createAcceptedConditionHistory(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateAcceptedConditionHistoryInput,
) {
  return apiPost<AcceptedConditionHistory, CreateAcceptedConditionHistoryInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/accepted-history`,
    input,
    "Could not create accepted-condition history.",
  );
}