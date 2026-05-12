import {
  GarpMQuestionGroupKey,
  GarpMQuestionGroupTemplate,
  GarpMQuestionTemplate,
  GarpMQuestionTemplateSet,
  garpMQuestionSafetyBoundary,
} from "@/lib/garpM/questionTemplateModel";
import { diagnosisSymptomsTreatmentQuestions } from "@/lib/garpM/questionTemplates/diagnosisSymptomsTreatment";
import { stabilityTreatmentResponseQuestions } from "@/lib/garpM/questionTemplates/stabilityTreatmentResponse";
import { functionalLifestyleWorkImpactQuestions } from "@/lib/garpM/questionTemplates/functionalLifestyleWorkImpact";
import { worseningPreviousCompensationQuestions } from "@/lib/garpM/questionTemplates/worseningPreviousCompensation";
import { evidenceAppointmentPrepQuestions } from "@/lib/garpM/questionTemplates/evidenceAppointmentPrep";

export type GarpMQuestionGroupMetadata = {
  groupKey: GarpMQuestionGroupKey;
  routeSegment: string;
  title: string;
  shortTitle: string;
  description: string;
  whyThisMatters: string;
  displayOrder: number;
  safetyNote: string;
};

export const garpMQuestionGroupMetadata: GarpMQuestionGroupMetadata[] = [
  {
    groupKey: "DIAGNOSIS_SYMPTOMS_TREATMENT",
    routeSegment: "diagnosis-symptoms-treatment",
    title: "Diagnosis, symptoms and treatment",
    shortTitle: "Diagnosis and symptoms",
    description:
      "Capture the condition name, diagnosis status, current symptoms, treatment history, medication and side effects.",
    whyThisMatters:
      "This helps organise the current clinical picture before speaking with a doctor, advocate, lawyer or support person.",
    displayOrder: 10,
    safetyNote: garpMQuestionSafetyBoundary,
  },
  {
    groupKey: "STABILITY_TREATMENT_RESPONSE",
    routeSegment: "stability-treatment-response",
    title: "Stability and treatment response",
    shortTitle: "Stability",
    description:
      "Capture whether the condition is stable, improving, worsening or fluctuating, and how treatment affects symptoms.",
    whyThisMatters:
      "This helps identify whether the evidence describes the current state of the condition clearly enough.",
    displayOrder: 20,
    safetyNote: garpMQuestionSafetyBoundary,
  },
  {
    groupKey: "FUNCTIONAL_LIFESTYLE_WORK_IMPACT",
    routeSegment: "functional-lifestyle-work-impact",
    title: "Functional, lifestyle and work impact",
    shortTitle: "Impact",
    description:
      "Capture how the condition affects daily activities, self-care, mobility, sleep, relationships, social participation and work.",
    whyThisMatters:
      "This helps turn symptoms into practical examples of day-to-day impact without calculating impairment points.",
    displayOrder: 30,
    safetyNote: garpMQuestionSafetyBoundary,
  },
  {
    groupKey: "WORSENING_PREVIOUS_COMPENSATION",
    routeSegment: "worsening-previous-compensation",
    title: "Worsening and previous compensation",
    shortTitle: "Worsening history",
    description:
      "Capture previous DVA acceptance, previous compensation or assessment history, worsening since prior decisions, and available letters.",
    whyThisMatters:
      "This helps organise background history where a condition may already have been accepted, assessed or compensated.",
    displayOrder: 40,
    safetyNote: garpMQuestionSafetyBoundary,
  },
  {
    groupKey: "EVIDENCE_APPOINTMENT_PREP",
    routeSegment: "evidence-appointment-prep",
    title: "Evidence gaps and appointment preparation",
    shortTitle: "Evidence and appointments",
    description:
      "Capture what evidence exists, what is missing, what needs to be requested, and what questions should be asked at appointments.",
    whyThisMatters:
      "This helps prepare for conversations with doctors, advocates, lawyers or support people.",
    displayOrder: 50,
    safetyNote: garpMQuestionSafetyBoundary,
  },
  {
    groupKey: "STRUCTURED_SUMMARY",
    routeSegment: "structured-summary",
    title: "Structured summary",
    shortTitle: "Summary",
    description:
      "Review saved answers, missing information and a plain-English preparation summary.",
    whyThisMatters:
      "This helps the user review and edit information before using it in any draft or document.",
    displayOrder: 60,
    safetyNote: garpMQuestionSafetyBoundary,
  },
];

export const orderedGarpMQuestionGroupKeys = garpMQuestionGroupMetadata
  .slice()
  .sort((a, b) => a.displayOrder - b.displayOrder)
  .map((group) => group.groupKey);

const questionsByGroup: Partial<Record<GarpMQuestionGroupKey, GarpMQuestionTemplate[]>> = {
  DIAGNOSIS_SYMPTOMS_TREATMENT: diagnosisSymptomsTreatmentQuestions,
  STABILITY_TREATMENT_RESPONSE: stabilityTreatmentResponseQuestions,
  FUNCTIONAL_LIFESTYLE_WORK_IMPACT: functionalLifestyleWorkImpactQuestions,
  WORSENING_PREVIOUS_COMPENSATION: worseningPreviousCompensationQuestions,
  EVIDENCE_APPOINTMENT_PREP: evidenceAppointmentPrepQuestions,
};

export function createGarpMQuestionGroups(): GarpMQuestionGroupTemplate[] {
  return garpMQuestionGroupMetadata
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder)
    .map((group) => ({
      groupKey: group.groupKey,
      title: group.title,
      description: group.description,
      displayOrder: group.displayOrder,
      safetyNote: group.safetyNote,
      questions: questionsByGroup[group.groupKey] ?? [],
    }));
}

export const garpMQuestionGroupTemplateSet: GarpMQuestionTemplateSet = {
  templateVersion: "garp-m-aware-web-v1",
  title: "GARP M-aware question engine",
  description:
    "A structured question engine for condition information, symptoms, treatment, stability, impact, worsening history, evidence gaps and appointment preparation.",
  safetyBoundary: garpMQuestionSafetyBoundary,
  groups: createGarpMQuestionGroups(),
};

export function getGarpMQuestionGroupMetadata(groupKey: GarpMQuestionGroupKey) {
  return garpMQuestionGroupMetadata.find((group) => group.groupKey === groupKey) ?? null;
}

export function getNextGarpMQuestionGroupKey(groupKey: GarpMQuestionGroupKey) {
  const index = orderedGarpMQuestionGroupKeys.indexOf(groupKey);

  if (index < 0 || index >= orderedGarpMQuestionGroupKeys.length - 1) {
    return null;
  }

  return orderedGarpMQuestionGroupKeys[index + 1];
}

export function getPreviousGarpMQuestionGroupKey(groupKey: GarpMQuestionGroupKey) {
  const index = orderedGarpMQuestionGroupKeys.indexOf(groupKey);

  if (index <= 0) {
    return null;
  }

  return orderedGarpMQuestionGroupKeys[index - 1];
}
