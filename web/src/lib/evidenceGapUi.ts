export const gapStatusOptions = [
  {
    value: "OPEN",
    label: "Open",
    description: "This gap still needs attention.",
  },
  {
    value: "IN_PROGRESS",
    label: "In progress",
    description: "The user is working on this evidence gap.",
  },
  {
    value: "RESOLVED",
    label: "Resolved",
    description: "The user has dealt with this gap for preparation purposes.",
  },
  {
    value: "USER_MARKED_NOT_APPLICABLE",
    label: "Not applicable",
    description: "The user has marked this gap as not applicable.",
  },
];

export const gapSeverityLabels: Record<string, string> = {
  LOW: "Low",
  MEDIUM: "Medium",
  HIGH: "High",
};

export const gapTypeLabels: Record<string, string> = {
  DIAGNOSIS_EVIDENCE_MISSING: "Diagnosis evidence missing",
  CURRENT_TREATMENT_EVIDENCE_MISSING: "Current treatment evidence missing",
  MEDICATION_EVIDENCE_MISSING: "Medication evidence missing",
  FUNCTIONAL_IMPACT_NOTES_MISSING: "Functional impact notes missing",
  PREVIOUS_DVA_DECISION_LETTER_MISSING: "Previous DVA decision letter missing",
  PREVIOUS_ASSESSMENT_LETTER_MISSING: "Previous assessment letter missing",
  WORSENING_EVIDENCE_MISSING: "Worsening evidence missing",
  GARP_M_EVIDENCE_FOLLOW_UP_RECORDED: "GARP M evidence follow-up recorded",
};

export function getEvidenceGapStatusLabel(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return gapStatusOptions.find((option) => option.value === value)?.label ?? value;
}

export function getEvidenceGapSeverityLabel(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return gapSeverityLabels[value] ?? value;
}

export function getEvidenceGapTypeLabel(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return gapTypeLabels[value] ?? value;
}