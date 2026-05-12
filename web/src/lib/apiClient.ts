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
export type QuestionResponse = {
  id: string;
  claimWorkspaceId: string;
  conditionId: string;
  questionGroup: string;
  questionKey: string;
  questionText: string;
  answerText?: string | null;
  answerType: string;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateQuestionResponseInput = {
  questionGroup: string;
  questionKey: string;
  questionText: string;
  answerText?: string;
  answerType: string;
};

export async function getQuestionResponses(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/question-responses`,
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
    await handleApiError(response, "Could not load guided question responses.");
  }

  return (await response.json()) as QuestionResponse[];
}

export async function createQuestionResponse(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateQuestionResponseInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/question-responses`,
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
    await handleApiError(response, "Could not save guided question response.");
  }

  return (await response.json()) as QuestionResponse;
}

export type EvidenceItem = {
  id: string;
  claimWorkspaceId: string;
  conditionId?: string | null;
  evidenceType: string;
  evidenceStatus: string;
  originalFileName?: string | null;
  storagePath?: string | null;
  fileType?: string | null;
  fileSize?: number | null;
  documentDate?: string | null;
  providerName?: string | null;
  userNotes?: string | null;
  aiSummary?: string | null;
  userConfirmedSummary?: string | null;
  usedInGeneratedPack: boolean;
  uploadedAt?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type CreateEvidenceItemInput = {
  evidenceType: string;
  evidenceStatus: string;
  originalFileName?: string;
  storagePath?: string;
  fileType?: string;
  fileSize?: number;
  documentDate?: string;
  providerName?: string;
  userNotes?: string;
  aiSummary?: string;
  userConfirmedSummary?: string;
  usedInGeneratedPack?: boolean;
};

export async function getConditionEvidenceItems(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-items`,
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
    await handleApiError(response, "Could not load evidence items.");
  }

  return (await response.json()) as EvidenceItem[];
}

export async function createEvidenceItem(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateEvidenceItemInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-items`,
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
    await handleApiError(response, "Could not create evidence item.");
  }

  return (await response.json()) as EvidenceItem;
}
export type EvidenceUploadUrlResponse = {
  evidenceItem: EvidenceItem;
  upload: {
    method: string;
    url: string;
    expiresInMinutes: number;
    requiredHeaders?: Record<string, string>;
    note?: string;
  };
};

export type CreateEvidenceUploadUrlInput = {
  evidenceType: string;
  originalFileName: string;
  fileType?: string;
  fileSize?: number;
  documentDate?: string;
  providerName?: string;
  userNotes?: string;
};

export async function createEvidenceUploadUrl(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateEvidenceUploadUrlInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-upload-url`,
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
    await handleApiError(response, "Could not create evidence upload URL.");
  }

  return (await response.json()) as EvidenceUploadUrlResponse;
}

export async function markEvidenceUploaded(
  idToken: string,
  workspaceId: string,
  evidenceItemId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/evidence-items/${evidenceItemId}/mark-uploaded`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
      },
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not confirm evidence upload.");
  }

  return (await response.json()) as EvidenceItem;
}
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

export async function getWorkspaceEvidenceGaps(
  idToken: string,
  workspaceId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/evidence-gaps`,
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
    await handleApiError(response, "Could not load evidence gaps.");
  }

  return (await response.json()) as EvidenceGap[];
}

export async function getConditionEvidenceGaps(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps`,
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
    await handleApiError(response, "Could not load condition evidence gaps.");
  }

  return (await response.json()) as EvidenceGap[];
}

export async function recalculateEvidenceGaps(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps/recalculate`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
      },
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not recalculate evidence gaps.");
  }

  return (await response.json()) as RecalculateEvidenceGapsResponse;
}

export async function updateEvidenceGap(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  gapId: string,
  input: UpdateEvidenceGapInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/evidence-gaps/${gapId}`,
    {
      method: "PATCH",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not update evidence gap.");
  }

  return (await response.json()) as EvidenceGap;
}
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

export async function getWorkspaceAiDrafts(
  idToken: string,
  workspaceId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/ai-drafts`,
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
    await handleApiError(response, "Could not load AI drafts.");
  }

  return (await response.json()) as AiDraft[];
}

export async function getConditionAiDrafts(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/ai-drafts`,
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
    await handleApiError(response, "Could not load condition AI drafts.");
  }

  return (await response.json()) as AiDraft[];
}

export async function createAiDraft(
  idToken: string,
  workspaceId: string,
  input: CreateAiDraftInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/ai-drafts`,
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
    await handleApiError(response, "Could not create AI draft.");
  }

  return (await response.json()) as AiDraft;
}

export async function updateAiDraft(
  idToken: string,
  workspaceId: string,
  draftId: string,
  input: UpdateAiDraftInput,
) {
  const response = await fetch(
    `${env.apiBaseUrl}/api/v1/claim-workspaces/${workspaceId}/ai-drafts/${draftId}`,
    {
      method: "PATCH",
      headers: {
        Authorization: `Bearer ${idToken}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
    },
  );

  if (!response.ok) {
    await handleApiError(response, "Could not update AI draft.");
  }

  return (await response.json()) as AiDraft;
}