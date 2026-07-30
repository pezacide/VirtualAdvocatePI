using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class StabilityTreatmentResponseQuestions
{
    private const string GroupKey = "STABILITY_TREATMENT_RESPONSE";

    public static IReadOnlyList<GarpMQuestionTemplate> All { get; } = new List<GarpMQuestionTemplate>
    {
        new()
        {
            Id = "str_current_stability",
            GroupKey = GroupKey,
            QuestionKey = "current_stability",
            QuestionText = "How would you describe the current stability of this condition?",
            HelperText = "Choose the closest option. This helps organise whether the condition is stable, improving, worsening or changing over time.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 10,
            EvidenceCategory = "STABILITY",
            SummaryLabel = "Current stability",
            Options = GarpMOptionSets.Stability,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "str_stability_explanation",
            GroupKey = GroupKey,
            QuestionKey = "stability_explanation",
            QuestionText = "Why did you choose that stability option?",
            HelperText = "Describe what has changed, what has stayed the same, and whether symptoms come and go.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 20,
            EvidenceCategory = "STABILITY",
            SummaryLabel = "Stability explanation",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "str_symptom_pattern",
            GroupKey = GroupKey,
            QuestionKey = "symptom_pattern",
            QuestionText = "Do symptoms follow a pattern?",
            HelperText = "Examples include morning symptoms, night symptoms, flare-ups, symptoms after activity, symptoms under stress, or symptoms that vary without a clear trigger.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 30,
            EvidenceCategory = "SYMPTOMS",
            SummaryLabel = "Symptom pattern",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "str_treatment_helping",
            GroupKey = GroupKey,
            QuestionKey = "treatment_helping",
            QuestionText = "Is current treatment helping?",
            HelperText = "This helps capture whether treatment is improving symptoms, partly helping, not helping, or making things difficult.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 40,
            EvidenceCategory = "TREATMENT",
            SummaryLabel = "Treatment response",
            Options = new List<GarpMQuestionOption>
            {
                new() { Value = "HELPING", Label = "Yes, treatment is helping" },
                new() { Value = "PARTLY_HELPING", Label = "Partly helping" },
                new() { Value = "NOT_HELPING", Label = "Not helping" },
                new() { Value = "MAKING_WORSE", Label = "Treatment causes problems or side effects" },
                new() { Value = "NO_CURRENT_TREATMENT", Label = "No current treatment" },
                new() { Value = "UNSURE", Label = "Unsure" },
            },
            SafetyNote = "Do not start, stop or change treatment based on this app. Speak with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "str_treatment_response_details",
            GroupKey = GroupKey,
            QuestionKey = "treatment_response_details",
            QuestionText = "What happens when you receive treatment or support?",
            HelperText = "Describe what improves, what does not improve, how long benefits last, and whether treatment has limits.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 50,
            EvidenceCategory = "TREATMENT",
            SummaryLabel = "Treatment response details",
            SafetyNote = "Treatment concerns should be discussed with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "str_treatment_changes",
            GroupKey = GroupKey,
            QuestionKey = "treatment_changes",
            QuestionText = "Has treatment changed over time?",
            HelperText = "Examples include increased medication, new referrals, changed therapy, more frequent appointments, or stopped treatment.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 60,
            EvidenceCategory = "TREATMENT",
            SummaryLabel = "Treatment changes",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "Do not start, stop or change treatment based on this app. Speak with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "str_treatment_change_details",
            GroupKey = GroupKey,
            QuestionKey = "treatment_change_details",
            QuestionText = "If treatment has changed, what changed and why?",
            HelperText = "This may help identify useful medical records, referrals, medication changes, specialist reviews or treatment plans.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 70,
            EvidenceCategory = "TREATMENT",
            SummaryLabel = "Treatment change details",
            SafetyNote = "Treatment concerns should be discussed with a doctor, pharmacist or qualified clinician.",
        },
        new()
        {
            Id = "str_condition_maximum_improvement",
            GroupKey = GroupKey,
            QuestionKey = "condition_maximum_improvement",
            QuestionText = "Has a doctor told you the condition is stable or unlikely to improve much more?",
            HelperText = "Only record what you have been told. This app does not decide whether a condition is stable or permanent.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 80,
            EvidenceCategory = "STABILITY",
            SummaryLabel = "Doctor comments about stability",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "This app does not make medical findings about stability, permanence or maximum improvement.",
        },
        new()
        {
            Id = "str_stability_evidence_available",
            GroupKey = GroupKey,
            QuestionKey = "stability_evidence_available",
            QuestionText = "What evidence shows the current stability or treatment response?",
            HelperText = "Examples include GP notes, specialist reports, treatment plans, medication history, allied health notes, progress notes or review letters.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 90,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Stability and treatment evidence",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "str_stability_questions_for_doctor",
            GroupKey = GroupKey,
            QuestionKey = "stability_questions_for_doctor",
            QuestionText = "What questions should you ask your doctor about stability or treatment response?",
            HelperText = "Write down anything you want to clarify at the next appointment.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 100,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Doctor questions about stability or treatment",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
    };
}
