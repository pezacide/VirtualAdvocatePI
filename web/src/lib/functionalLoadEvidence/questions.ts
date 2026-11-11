// Functional and Load Evidence question bank.
//
// Client-side template set, persisted through the generic question-responses API
// with the key prefix `fle:` and questionGroup `FUNCTIONAL_LOAD_EVIDENCE`.
// Preparation support only - this does not calculate impairment or capacity.

export type FleAnswerType =
  | "TEXT"
  | "LONG_TEXT"
  | "NUMBER"
  | "DATE"
  | "YES_NO_UNSURE"
  | "SINGLE_SELECT";

export type FleQuestionOption = { value: string; label: string };

export type FleQuestion = {
  id: string;
  questionKey: string;
  questionText: string;
  helperText?: string;
  answerType: FleAnswerType;
  required?: boolean;
  options?: FleQuestionOption[];
};

export type FleQuestionGroup = {
  groupKey: string;
  title: string;
  description: string;
  questions: FleQuestion[];
};

export const fleSafetyBoundary =
  "This tool helps organise information about physically demanding service and current function for preparation only. " +
  "It does not calculate GARP M impairment points, assess work capacity, estimate compensation, provide legal or medical " +
  "advice, make a DVA decision, or guarantee a claim outcome.";

const yesNoUnsure: FleQuestionOption[] = [
  { value: "YES", label: "Yes" },
  { value: "NO", label: "No" },
  { value: "UNSURE", label: "Unsure" },
];

export const fleQuestionGroups: FleQuestionGroup[] = [
  {
    groupKey: "PHYSICAL_DEMANDS_HISTORY",
    title: "Physically demanding service history",
    description:
      "Describe the physically demanding parts of your service that may be relevant to this condition.",
    questions: [
      {
        id: "fle_roles",
        questionKey: "physical_roles",
        questionText: "Which roles, trades or postings involved heavy physical work?",
        helperText: "List the roles and roughly when you held them.",
        answerType: "LONG_TEXT",
        required: true,
      },
      {
        id: "fle_typical_tasks",
        questionKey: "typical_physical_tasks",
        questionText: "What physical tasks did you do routinely?",
        helperText:
          "For example: lifting stores, carrying webbing and packs, climbing ladders, working from kneeling, damage control drills.",
        answerType: "LONG_TEXT",
        required: true,
      },
      {
        id: "fle_years_demanding",
        questionKey: "years_physically_demanding",
        questionText: "Roughly how many years of service were physically demanding?",
        answerType: "NUMBER",
      },
      {
        id: "fle_heaviest",
        questionKey: "heaviest_regular_load",
        questionText: "What was the heaviest load you regularly lifted or carried, and how often?",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_notable_events",
        questionKey: "notable_load_events",
        questionText: "Were there specific incidents where a load caused pain or injury?",
        helperText: "Include approximate dates and whether they were reported or treated.",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_ongoing_since_service",
        questionKey: "symptoms_since_service",
        questionText: "Have symptoms in this body area continued since that service?",
        answerType: "YES_NO_UNSURE",
        options: yesNoUnsure,
      },
    ],
  },
  {
    groupKey: "CURRENT_FUNCTIONAL_CAPACITY",
    title: "Current functional capacity",
    description:
      "Describe how the condition affects what you can physically do now, in plain language.",
    questions: [
      {
        id: "fle_current_lifting_limit",
        questionKey: "current_lifting_limit",
        questionText: "What can you lift or carry comfortably now, and what causes a flare-up?",
        answerType: "LONG_TEXT",
        required: true,
      },
      {
        id: "fle_positions_hard",
        questionKey: "difficult_positions",
        questionText: "Which positions or movements are now difficult or painful?",
        helperText: "For example: bending, kneeling, squatting, reaching overhead, stairs, sustained standing.",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_aids_used",
        questionKey: "aids_or_modifications",
        questionText: "Do you use aids, equipment or task modifications?",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_help_from_others",
        questionKey: "help_from_others",
        questionText: "What tasks do you now need help from someone else to do?",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_work_effect",
        questionKey: "effect_on_work",
        questionText: "How has this affected paid work or the type of work you can do?",
        answerType: "LONG_TEXT",
      },
    ],
  },
  {
    groupKey: "LOAD_EVIDENCE_PREP",
    title: "Evidence and appointment preparation",
    description:
      "Note what evidence you have, what is missing, and what to raise at appointments.",
    questions: [
      {
        id: "fle_have_service_docs",
        questionKey: "have_service_records",
        questionText: "Do you have service records that show your role and its physical demands?",
        answerType: "YES_NO_UNSURE",
        options: yesNoUnsure,
      },
      {
        id: "fle_witnesses",
        questionKey: "witness_statements",
        questionText: "Are there people who could describe the physical work you did?",
        helperText: "Names or roles are enough at this stage.",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_missing_evidence",
        questionKey: "missing_load_evidence",
        questionText: "What evidence about the physical demands is missing or needs requesting?",
        answerType: "LONG_TEXT",
      },
      {
        id: "fle_doctor_points",
        questionKey: "points_for_doctor",
        questionText: "What points about physical demands and current function should a doctor hear?",
        answerType: "LONG_TEXT",
      },
    ],
  },
];

export const fleAllQuestions = fleQuestionGroups.flatMap((group) => group.questions);
