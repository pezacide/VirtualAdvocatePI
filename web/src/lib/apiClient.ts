import { env } from "@/lib/env";

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

async function handleApiError(response: Response, defaultMessage: string) {
  if (response.status === 401) {
    throw new Error("You are not signed in or your session has expired.");
  }

  const errorText = await response.text();

  throw new Error(`${defaultMessage} HTTP ${response.status}. ${errorText}`);
}

export async function getClaimWorkspaces(idToken: string) {
  const response = await fetch(`${env.apiBaseUrl}/api/v1/claim-workspaces`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${idToken}`,
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    await handleApiError(response, "Could not load claim workspaces.");
  }

  return (await response.json()) as ClaimWorkspace[];
}

export async function getClaimWorkspace(idToken: string, workspaceId: string) {
  const response = await fetch(`${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${idToken}`,
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (response.status === 404) {
    throw new Error("Claim workspace was not found.");
  }

  if (!response.ok) {
    await handleApiError(response, "Could not load claim workspace.");
  }

  return (await response.json()) as ClaimWorkspace;
}

export async function createClaimWorkspace(
  idToken: string,
  input: CreateClaimWorkspaceInput,
) {
  const response = await fetch(`${env.apiBaseUrl}/api/v1/claim-workspaces`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${idToken}`,
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    await handleApiError(response, "Could not create claim workspace.");
  }

  return (await response.json()) as ClaimWorkspace;
}