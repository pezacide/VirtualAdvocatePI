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
  LISTED: "Listed",
  PENDING_UPLOAD: "Pending upload",
  UPLOADED: "Uploaded",
  NEEDS_REVIEW: "Needs review",
  REVIEWED: "Reviewed",
  READY: "Ready",
  USED_IN_PACK: "Used in pack",
  ARCHIVED: "Archived",
};

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