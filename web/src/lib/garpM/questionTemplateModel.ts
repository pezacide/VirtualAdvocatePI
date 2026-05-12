export type GarpMQuestionAnswerType =
  | "TEXT"
  | "LONG_TEXT"
  | "YES_NO"
  | "YES_NO_UNSURE"
  | "DATE"
  | "NUMBER"
  | "SINGLE_SELECT"
  | "MULTI_SELECT";

export type GarpMQuestionGroupKey =
  | "DIAGNOSIS_SYMPTOMS_TREATMENT"
  | "STABILITY_TREATMENT_RESPONSE"
  | "FUNCTIONAL_LIFESTYLE_WORK_IMPACT"
  | "WORSENING_PREVIOUS_COMPENSATION"
  | "EVIDENCE_APPOINTMENT_PREP"
  | "STRUCTURED_SUMMARY";

export type GarpMQuestionRequirementLevel = "REQUIRED" | "RECOMMENDED" | "OPTIONAL";

export type GarpMQuestionScope = "WORKSPACE" | "CONDITION";

export type GarpMEvidenceCategory =
  | "DIAGNOSIS"
  | "SYMPTOMS"
  | "TREATMENT"
  | "MEDICATION"
  | "STABILITY"
  | "FUNCTIONAL_IMPACT"
  | "LIFESTYLE_IMPACT"
  | "WORK_IMPACT"
  | "WORSENING"
  | "PREVIOUS_COMPENSATION"
  | "SERVICE_CONNECTION"
  | "EVIDENCE_GAP"
  | "APPOINTMENT_PREP"
  | "SUMMARY";

export type GarpMValidationRule = {
  type: "MIN_LENGTH" | "MAX_LENGTH" | "REQUIRED_WHEN" | "NONE";
  value?: number | string;
  message?: string;
};

export type GarpMQuestionOption = {
  value: string;
  label: string;
  helperText?: string;
};

export type GarpMQuestionTemplate = {
  id: string;
  groupKey: GarpMQuestionGroupKey;
  questionKey: string;
  questionText: string;
  helperText: string;
  answerType: GarpMQuestionAnswerType;
  requirementLevel: GarpMQuestionRequirementLevel;
  scope: GarpMQuestionScope;
  displayOrder: number;
  evidenceCategory: GarpMEvidenceCategory;
  summaryLabel: string;
  options?: GarpMQuestionOption[];
  validationRules?: GarpMValidationRule[];
  safetyNote?: string;
};

export type GarpMQuestionGroupTemplate = {
  groupKey: GarpMQuestionGroupKey;
  title: string;
  description: string;
  displayOrder: number;
  safetyNote: string;
  questions: GarpMQuestionTemplate[];
};

export type GarpMQuestionTemplateSet = {
  templateVersion: string;
  title: string;
  description: string;
  safetyBoundary: string;
  groups: GarpMQuestionGroupTemplate[];
};

export const yesNoUnsureOptions: GarpMQuestionOption[] = [
  { value: "YES", label: "Yes" },
  { value: "NO", label: "No" },
  { value: "UNSURE", label: "Unsure" },
];

export const stabilityOptions: GarpMQuestionOption[] = [
  { value: "STABLE", label: "Stable" },
  { value: "IMPROVING", label: "Improving" },
  { value: "WORSENING", label: "Worsening" },
  { value: "FLUCTUATING", label: "Fluctuating" },
  { value: "UNSURE", label: "Unsure" },
];

export const impactFrequencyOptions: GarpMQuestionOption[] = [
  { value: "DAILY", label: "Daily" },
  { value: "MOST_DAYS", label: "Most days" },
  { value: "WEEKLY", label: "Weekly" },
  { value: "OCCASIONAL", label: "Occasional" },
  { value: "FLARE_UPS_ONLY", label: "During flare-ups only" },
  { value: "UNSURE", label: "Unsure" },
];

export function getQuestionsForGroup(
  templateSet: GarpMQuestionTemplateSet,
  groupKey: GarpMQuestionGroupKey,
) {
  return (
    templateSet.groups
      .find((group) => group.groupKey === groupKey)
      ?.questions.slice()
      .sort((a, b) => a.displayOrder - b.displayOrder) ?? []
  );
}

export function getAllQuestions(templateSet: GarpMQuestionTemplateSet) {
  return templateSet.groups
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder)
    .flatMap((group) =>
      group.questions
        .slice()
        .sort((a, b) => a.displayOrder - b.displayOrder),
    );
}

export function getRequiredQuestions(templateSet: GarpMQuestionTemplateSet) {
  return getAllQuestions(templateSet).filter(
    (question) => question.requirementLevel === "REQUIRED",
  );
}

export function getQuestionById(
  templateSet: GarpMQuestionTemplateSet,
  questionId: string,
) {
  return getAllQuestions(templateSet).find((question) => question.id === questionId) ?? null;
}

export const garpMQuestionSafetyBoundary =
  "This feature helps organise information for preparation only. It does not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.";
