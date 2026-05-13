export const evidenceTypeLabels: Record<string, string> = {
  MEDICAL_REPORT: "Medical report",
  GP_REPORT: "GP report",
  SPECIALIST_REPORT: "Specialist report",
  ALLIED_HEALTH_REPORT: "Allied health report",
  IMAGING_REPORT: "Imaging report",
  TEST_RESULT: "Test result",
  DVA_LETTER: "DVA letter",
  SERVICE_RECORD: "Service record",
  PERSONAL_STATEMENT: "Personal statement",
  SUPPORTING_STATEMENT: "Supporting statement",
  EMPLOYMENT_RECORD: "Employment record",
  MEDICATION_LIST: "Medication list",
  TREATMENT_PLAN: "Treatment plan",
  OTHER: "Other evidence",
};

export const evidenceStatusLabels: Record<string, string> = {
  MISSING: "Missing",
  LISTED_NOT_UPLOADED: "Listed, not uploaded",
  UPLOADED: "Uploaded",
  REVIEWED: "Reviewed",
  CONFIRMED: "Confirmed",
  NOT_APPLICABLE: "Not applicable",
};

export const evidenceTypeOptions = [
  { value: "MEDICAL_REPORT", label: "Medical report", category: "Medical" },
  { value: "GP_REPORT", label: "GP report", category: "Medical" },
  { value: "SPECIALIST_REPORT", label: "Specialist report", category: "Medical" },
  { value: "ALLIED_HEALTH_REPORT", label: "Allied health report", category: "Medical" },
  { value: "IMAGING_REPORT", label: "Imaging report", category: "Medical" },
  { value: "TEST_RESULT", label: "Test result", category: "Medical" },
  { value: "MEDICATION_LIST", label: "Medication list", category: "Treatment" },
  { value: "TREATMENT_PLAN", label: "Treatment plan", category: "Treatment" },
  { value: "DVA_LETTER", label: "DVA letter", category: "DVA" },
  { value: "SERVICE_RECORD", label: "Service record", category: "Service" },
  { value: "PERSONAL_STATEMENT", label: "Personal statement", category: "Statement" },
  { value: "SUPPORTING_STATEMENT", label: "Supporting statement", category: "Statement" },
  { value: "EMPLOYMENT_RECORD", label: "Employment record", category: "Work" },
  { value: "OTHER", label: "Other evidence", category: "Other" },
];

export const evidenceSourceQuickTags = [
  "GP",
  "Specialist",
  "Allied health provider",
  "Hospital",
  "DVA",
  "Open Arms",
  "ADF / service record",
  "Employer",
  "Personal notes",
  "Family or support person",
];

export function getEvidenceTypeLabel(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return evidenceTypeLabels[value] ?? value;
}

export function getEvidenceStatusLabel(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return evidenceStatusLabels[value] ?? value;
}

export function getEvidenceTypeCategory(value?: string | null) {
  if (!value) {
    return "Not recorded";
  }

  return evidenceTypeOptions.find((option) => option.value === value)?.category ?? "Other";
}
export const evidenceStatusOptions = [
  {
    value: "MISSING",
    label: "Missing",
    description: "Evidence is known to be missing or still needs to be obtained.",
  },
  {
    value: "LISTED_NOT_UPLOADED",
    label: "Listed, not uploaded",
    description: "Evidence has been identified but no file has been uploaded yet.",
  },
  {
    value: "UPLOADED",
    label: "Uploaded",
    description: "A file has been uploaded for this evidence item.",
  },
  {
    value: "REVIEWED",
    label: "Reviewed",
    description: "The user has reviewed this evidence item for preparation purposes.",
  },
  {
    value: "CONFIRMED",
    label: "Confirmed",
    description: "The user has confirmed this evidence item is ready for preparation use.",
  },
  {
    value: "NOT_APPLICABLE",
    label: "Not applicable",
    description: "This evidence item is not applicable to the current preparation workflow.",
  },
];