import { apiGet, apiPost, getAuthHeaders, getApiBaseUrl, handleApiError } from "@/lib/api/client";

export type ClaimCondition = {
  id: string;
  claimWorkspaceId: string;
  conditionName: string;
  diagnosisStatus: string;
  dateDiagnosed?: string | null;
  currentSymptoms?: string | null;
  treatmentSummary?: string | null;
  medicationSummary?: string | null;
  medicationSideEffects?: string | null;
  functionalImpactSummary?: string | null;
  lifestyleImpactSummary?: string | null;
  workImpactSummary?: string | null;
  stabilityNotes?: string | null;
  worseningNotes?: string | null;
  isPrimaryCondition: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateConditionInput = {
  conditionName: string;
  diagnosisStatus: string;
  dateDiagnosed?: string;
  currentSymptoms?: string;
  treatmentSummary?: string;
  medicationSummary?: string;
  medicationSideEffects?: string;
  functionalImpactSummary?: string;
  lifestyleImpactSummary?: string;
  workImpactSummary?: string;
  stabilityNotes?: string;
  worseningNotes?: string;
  isPrimaryCondition?: boolean;
};

export function getClaimConditions(idToken: string, workspaceId: string) {
  return apiGet<ClaimCondition[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions`,
    "Could not load conditions.",
  );
}

export function createClaimCondition(
  idToken: string,
  workspaceId: string,
  input: CreateConditionInput,
) {
  return apiPost<ClaimCondition, CreateConditionInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions`,
    input,
    "Could not create condition.",
  );
}
export type ArchiveClaimConditionResponse = {
  id: string;
  status: string;
  archived: boolean;
};

export async function archiveClaimCondition(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}`,
    {
      method: "DELETE",
      headers: getAuthHeaders(idToken),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not remove condition from workspace.");
  }

  return (await response.json()) as ArchiveClaimConditionResponse;
}
