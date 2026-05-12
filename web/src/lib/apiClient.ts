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

export type CreateClaimWorkspaceInput = {
  workspaceTitle: string;
  claimScenario: string;
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

export async function getClaimConditions(idToken: string, workspaceId: string) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
      },
      cache: "no-store",
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not load conditions.");
  }

  return (await response.json()) as ClaimCondition[];
}

export async function createClaimCondition(
  idToken: string,
  workspaceId: string,
  input: CreateConditionInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not create condition.");
  }

  return (await response.json()) as ClaimCondition;
}

export async function getAcceptedConditionHistory(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/accepted-history`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
      },
      cache: "no-store",
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not load accepted-condition history.");
  }

  return (await response.json()) as AcceptedConditionHistory[];
}

export async function createAcceptedConditionHistory(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateAcceptedConditionHistoryInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/accepted-history`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not create accepted-condition history.");
  }

  return (await response.json()) as AcceptedConditionHistory;
}