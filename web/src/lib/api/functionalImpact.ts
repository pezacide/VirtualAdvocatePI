import {
  apiGet,
  apiPatch,
  apiPost,
  getApiBaseUrl,
  getAuthHeaders,
  handleApiError,
} from "@/lib/api/client";

export const functionalImpactActivityDomains = [
  "SELF_CARE",
  "MOBILITY",
  "LIFTING_CARRYING",
  "HOUSEHOLD",
  "WORK",
  "SLEEP",
  "SOCIAL",
  "DRIVING",
  "RECREATION",
  "OTHER",
] as const;

export const functionalImpactFrequencies = [
  "DAILY",
  "MOST_DAYS",
  "WEEKLY",
  "MONTHLY",
  "OCCASIONAL",
  "FLARE_UPS_ONLY",
  "UNSURE",
] as const;

export type FunctionalImpactEntry = {
  id: string;
  claimWorkspaceId: string;
  conditionId: string;
  activityDomain: string;
  goodDayDescription?: string | null;
  badDayDescription?: string | null;
  badDayFrequency: string;
  aidsOrHelpUsed?: string | null;
  notes?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateFunctionalImpactEntryInput = {
  activityDomain: string;
  goodDayDescription?: string;
  badDayDescription?: string;
  badDayFrequency?: string;
  aidsOrHelpUsed?: string;
  notes?: string;
};

export type UpdateFunctionalImpactEntryInput = Partial<CreateFunctionalImpactEntryInput>;

export function getFunctionalImpactEntries(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<FunctionalImpactEntry[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/functional-impact`,
    "Could not load functional impact entries.",
  );
}

export function createFunctionalImpactEntry(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateFunctionalImpactEntryInput,
) {
  return apiPost<FunctionalImpactEntry, CreateFunctionalImpactEntryInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/functional-impact`,
    input,
    "Could not add functional impact entry.",
  );
}

export function updateFunctionalImpactEntry(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  entryId: string,
  input: UpdateFunctionalImpactEntryInput,
) {
  return apiPatch<FunctionalImpactEntry, UpdateFunctionalImpactEntryInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/functional-impact/${entryId}`,
    input,
    "Could not update functional impact entry.",
  );
}

export async function archiveFunctionalImpactEntry(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  entryId: string,
) {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/functional-impact/${entryId}`,
    { method: "DELETE", headers: getAuthHeaders(idToken) },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not remove functional impact entry.");
  }

  return (await response.json()) as { id: string; status: string; archived: boolean };
}
