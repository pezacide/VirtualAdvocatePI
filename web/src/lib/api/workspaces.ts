import { apiGet, apiPost } from "@/lib/api/client";

export type ClaimWorkspace = {
  id: string;
  claimFramework: string;
  claimScenario: string;
  workspaceTitle: string;
  status: string;
  generatedPackStatus: string;
  createdAt: string;
  updatedAt: string;
  lastOpenedAt?: string | null;
};

export type CreateClaimWorkspaceInput = {
  workspaceTitle: string;
  claimScenario: string;
};

export function getClaimWorkspaces(idToken: string) {
  return apiGet<ClaimWorkspace[]>(
    idToken,
    "/api/v1/claim-workspaces",
    "Could not load claim workspaces.",
  );
}

export function getClaimWorkspace(idToken: string, workspaceId: string) {
  return apiGet<ClaimWorkspace>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}`,
    "Could not load claim workspace.",
  );
}

export function createClaimWorkspace(
  idToken: string,
  input: CreateClaimWorkspaceInput,
) {
  return apiPost<ClaimWorkspace, CreateClaimWorkspaceInput>(
    idToken,
    "/api/v1/claim-workspaces",
    input,
    "Could not create claim workspace.",
  );
}