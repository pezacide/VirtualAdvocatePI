using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class DiagnosisSymptomsTreatmentQuestions
{
    private const string GroupKey = "DIAGNOSIS_SYMPTOMS_TREATMENT";

    public static IReadOnlyList<GarpMQuestionTemplate> All { get; } = new List<GarpMQuestionTemplate>
    {
        new()
        {
            Id = "dst_condition_name",
            GroupKey = GroupKey,
            QuestionKey = "condition_name",
            QuestionText = "What condition are you preparing information about?",
            HelperText = "Use the name you normally use for the condition. This can be updated later if a doctor uses different wording.",
            AnswerType = GarpMAnswerTypes.Text,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 10,
            EvidenceCategory = "DIAGNOSIS",
            SummaryLabel = "Condition name",
            ValidationRules = new List<GarpMValidationRule>
            {
                new() { Type = "MIN_LENGTH", Value = 2, Message = "Enter at least a short condition name." },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_diagnosis_status",
            GroupKey = GroupKey,
            QuestionKey = "diagnosis_status",
            QuestionText = "Has this condition been diagnosed by a doctor or specialist?",
            HelperText = "This helps separate diagnosed, provisional, self-reported and uncertain information before evidence is organised.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 20,
            EvidenceCategory = "DIAGNOSIS",
            SummaryLabel = "Diagnosis status",
            Options = new List<GarpMQuestionOption>
            {
                new() { Value = "DIAGNOSED", Label = "Diagnosed" },
                new() { Value = "PROVISIONAL_DIAGNOSIS", Label = "Provisional diagnosis" },
                new() { Value = "SELF_REPORTED", Label = "Self-reported symptoms only" },
                new() { Value = "NOT_YET_DIAGNOSED", Label = "Not yet diagnosed" },
                new() { Value = "UNSURE", Label = "Unsure" },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_diagnosis_date",
            GroupKey = GroupKey,
            QuestionKey = "diagnosis_date",
            QuestionText = "When was the condition diagnosed, if known?",
            HelperText = "An approximate date is useful if known. Leave blank if you are unsure.",
            AnswerType = GarpMAnswerTypes.Date,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 30,
            EvidenceCategory = "DIAGNOSIS",
            SummaryLabel = "Diagnosis date",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_main_symptoms",
            GroupKey = GroupKey,
            QuestionKey = "main_symptoms",
            QuestionText = "What are the main symptoms you currently experience?",
            HelperText = "Describe symptoms in plain English. Include what happens, how it feels, and any common triggers.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 40,
            EvidenceCategory = "SYMPTOMS",
            SummaryLabel = "Main symptoms",
            ValidationRules = new List<GarpMValidationRule>
            {
                new() { Type = "MIN_LENGTH", Value = 10, Message = "Add a short plain-English description of the symptoms." },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_symptom_frequency",
            GroupKey = GroupKey,
            QuestionKey = "symptom_frequency",
            QuestionText = "How often do the symptoms occur?",
            HelperText = "This helps organise whether symptoms are constant, daily, frequent, occasional or flare-up based.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 50,
            EvidenceCategory = "SYMPTOMS",
            SummaryLabel = "Symptom frequency",
            Options = new List<GarpMQuestionOption>
            {
                new() { Value = "CONSTANT", Label = "Constant or nearly constant" },
                new() { Value = "DAILY", Label = "Daily" },
                new() { Value = "MOST_DAYS", Label = "Most days" },
                new() { Value = "WEEKLY", Label = "Weekly" },
                new() { Value = "OCCASIONAL", Label = "Occasional" },
                new() { Value = "FLARE_UPS_ONLY", Label = "During flare-ups only" },
                new() { Value = "UNSURE", Label = "Unsure" },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_symptom_severity_examples",
            GroupKey = GroupKey,
            QuestionKey = "symptom_severity_examples",
            QuestionText = "What does a bad day or flare-up look like?",
            HelperText = "Give practical examples. This is useful for explaining impact without trying to calculate a score.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 60,
            EvidenceCategory = "SYMPTOMS",
            SummaryLabel = "Bad day or flare-up examples",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_current_treatment",
            GroupKey = GroupKey,
            QuestionKey = "current_treatment",
            QuestionText = "What treatment or support are you currently receiving?",
            HelperText = "Include GP care, specialists, allied health, counselling, physiotherapy, hearing support, pain management or other treatment.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 70,
            EvidenceCategory = "TREATMENT",
            SummaryLabel = "Current treatment",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_medications",
            GroupKey = GroupKey,
            QuestionKey = "medications",
            QuestionText = "Are you taking medication for this condition?",
            HelperText = "List medication names if known. Do not change medication based on this app. Speak with a doctor or pharmacist.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 80,
            EvidenceCategory = "MEDICATION",
            SummaryLabel = "Medication",
            SafetyNote = "Do not start, stop or change medication based on this app. Speak with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "dst_medication_side_effects",
            GroupKey = GroupKey,
            QuestionKey = "medication_side_effects",
            QuestionText = "Do you experience side effects from medication or treatment?",
            HelperText = "Only record what you experience. This app does not provide medication advice.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 90,
            EvidenceCategory = "MEDICATION",
            SummaryLabel = "Medication or treatment side effects",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "Medication concerns should be discussed with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "dst_side_effect_details",
            GroupKey = GroupKey,
            QuestionKey = "side_effect_details",
            QuestionText = "If yes, what side effects or treatment problems do you notice?",
            HelperText = "Describe the side effects, when they happen, and how they affect daily life.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 100,
            EvidenceCategory = "MEDICATION",
            SummaryLabel = "Side effect details",
            SafetyNote = "Medication concerns should be discussed with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "dst_relevant_reports",
            GroupKey = GroupKey,
            QuestionKey = "relevant_reports",
            QuestionText = "What diagnosis, treatment or medical reports do you already have?",
            HelperText = "Examples include GP summaries, specialist reports, imaging reports, hearing tests, medication lists or treatment plans.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 110,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Available medical evidence",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "dst_missing_reports",
            GroupKey = GroupKey,
            QuestionKey = "missing_reports",
            QuestionText = "What diagnosis, treatment or medical evidence may still be missing?",
            HelperText = "This helps prepare appointment questions and evidence gap prompts.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 120,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Missing medical evidence",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
    };
}
