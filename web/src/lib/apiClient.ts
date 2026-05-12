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

export async function getClaimWorkspaces(idToken: string) {
  const response = await fetch(`${env.apiBaseUrl}/api/v1/claim-workspaces`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${idToken}`,
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (response.status === 401) {
    throw new Error("You are not signed in or your session has expired.");
  }

  if (!response.ok) {
    throw new Error(`Could not load claim workspaces. HTTP ${response.status}`);
  }

  return (await response.json()) as ClaimWorkspace[];
}