import { apiGet, apiPost } from "@/lib/api/client";

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

export function getQuestionResponses(
  idToken: string,
  workspaceId: string,
  conditionId: string,
) {
  return apiGet<QuestionResponse[]>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/question-responses`,
    "Could not load guided question responses.",
  );
}

export function createQuestionResponse(
  idToken: string,
  workspaceId: string,
  conditionId: string,
  input: CreateQuestionResponseInput,
) {
  return apiPost<QuestionResponse, CreateQuestionResponseInput>(
    idToken,
    `/api/v1/claim-workspaces/${workspaceId}/conditions/${conditionId}/question-responses`,
    input,
    "Could not save guided question response.",
  );
}