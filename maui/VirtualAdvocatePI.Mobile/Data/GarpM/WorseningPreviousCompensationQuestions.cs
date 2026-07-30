using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class WorseningPreviousCompensationQuestions
{
    private const string GroupKey = "WORSENING_PREVIOUS_COMPENSATION";

    public static IReadOnlyList<GarpMQuestionTemplate> All { get; } = new List<GarpMQuestionTemplate>
    {
        new()
        {
            Id = "wpc_previously_accepted",
            GroupKey = GroupKey,
            QuestionKey = "previously_accepted",
            QuestionText = "Has DVA previously accepted this condition?",
            HelperText = "Only record what you know from DVA letters or previous decisions. Choose unsure if you are not certain.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 10,
            EvidenceCategory = "PREVIOUS_COMPENSATION",
            SummaryLabel = "Previously accepted by DVA",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "This app does not confirm whether DVA has accepted a condition. Use DVA decision letters or professional advice where needed.",
        },
        new()
        {
            Id = "wpc_original_act",
            GroupKey = GroupKey,
            QuestionKey = "original_act",
            QuestionText = "Which Act was the condition previously accepted or assessed under, if known?",
            HelperText = "Examples include MRCA, DRCA or VEA. Choose unsure if you do not know.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 20,
            EvidenceCategory = "PREVIOUS_COMPENSATION",
            SummaryLabel = "Original Act",
            Options = new List<GarpMQuestionOption>
            {
                new() { Value = "MRCA", Label = "MRCA" },
                new() { Value = "DRCA", Label = "DRCA" },
                new() { Value = "VEA", Label = "VEA" },
                new() { Value = "MULTIPLE_OR_UNCLEAR", Label = "Multiple or unclear" },
                new() { Value = "UNSURE", Label = "Unsure" },
            },
            SafetyNote = "This app does not decide which Act applies. Check DVA letters or speak with an advocate, lawyer or qualified support person.",
        },
        new()
        {
            Id = "wpc_previous_compensation",
            GroupKey = GroupKey,
            QuestionKey = "previous_compensation",
            QuestionText = "Have you previously received compensation or assessment outcomes for this condition?",
            HelperText = "This may include previous PI, DCP, incapacity, review or assessment material. Choose unsure if you are not certain.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 30,
            EvidenceCategory = "PREVIOUS_COMPENSATION",
            SummaryLabel = "Previous compensation or assessment",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_decision_letter_available",
            GroupKey = GroupKey,
            QuestionKey = "decision_letter_available",
            QuestionText = "Do you have the previous DVA decision letter?",
            HelperText = "Decision letters can help confirm dates, accepted conditions, Act details and what was previously decided.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 40,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Previous DVA decision letter available",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_assessment_letter_available",
            GroupKey = GroupKey,
            QuestionKey = "assessment_letter_available",
            QuestionText = "Do you have any previous PI, DCP or assessment letter for this condition?",
            HelperText = "Assessment material can help organise what has changed since the previous assessment.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 50,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Previous assessment material available",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_previous_decision_date",
            GroupKey = GroupKey,
            QuestionKey = "previous_decision_date",
            QuestionText = "What was the date of the previous DVA decision, if known?",
            HelperText = "Leave blank if you do not know. The exact date can be checked later from DVA letters.",
            AnswerType = GarpMAnswerTypes.Date,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 60,
            EvidenceCategory = "PREVIOUS_COMPENSATION",
            SummaryLabel = "Previous decision date",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_previous_assessment_date",
            GroupKey = GroupKey,
            QuestionKey = "previous_assessment_date",
            QuestionText = "What was the date of the previous assessment, if known?",
            HelperText = "Leave blank if you do not know. The exact date can be checked later from assessment material.",
            AnswerType = GarpMAnswerTypes.Date,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 70,
            EvidenceCategory = "PREVIOUS_COMPENSATION",
            SummaryLabel = "Previous assessment date",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_worsening_claimed",
            GroupKey = GroupKey,
            QuestionKey = "worsening_claimed",
            QuestionText = "Are you preparing information because this condition may have worsened?",
            HelperText = "Choose the closest answer. This does not decide whether DVA will accept worsening.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 80,
            EvidenceCategory = "WORSENING",
            SummaryLabel = "Worsening claimed or being considered",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "This app does not decide whether a condition has worsened for DVA purposes. It only helps organise user-provided information.",
        },
        new()
        {
            Id = "wpc_worsening_summary",
            GroupKey = GroupKey,
            QuestionKey = "worsening_summary",
            QuestionText = "What has worsened or changed since the previous decision or assessment?",
            HelperText = "Describe changes in symptoms, treatment, stability, functional impact, lifestyle impact, work impact, medication, support needs or flare-ups.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 90,
            EvidenceCategory = "WORSENING",
            SummaryLabel = "Worsening summary",
            ValidationRules = new List<GarpMValidationRule>
            {
                new() { Type = "MIN_LENGTH", Value = 10, Message = "Add at least one plain-English example of what has changed." },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_worsening_timeline",
            GroupKey = GroupKey,
            QuestionKey = "worsening_timeline",
            QuestionText = "When did you first notice the worsening or change?",
            HelperText = "Use approximate timing if needed, such as a month, year, event, treatment change or period of worsening.",
            AnswerType = GarpMAnswerTypes.Text,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 100,
            EvidenceCategory = "WORSENING",
            SummaryLabel = "Worsening timeline",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_worsening_evidence",
            GroupKey = GroupKey,
            QuestionKey = "worsening_evidence",
            QuestionText = "What evidence may show the worsening or change?",
            HelperText = "Examples include updated GP notes, specialist reports, treatment changes, medication changes, imaging, hearing tests, leave records, support letters or personal notes.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 110,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Worsening evidence",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_missing_previous_documents",
            GroupKey = GroupKey,
            QuestionKey = "missing_previous_documents",
            QuestionText = "What previous DVA or assessment documents may still be missing?",
            HelperText = "List anything you may need to locate, request or discuss with an advocate, lawyer or support person.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 120,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Missing previous decision or assessment documents",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "wpc_questions_for_support",
            GroupKey = GroupKey,
            QuestionKey = "questions_for_support",
            QuestionText = "What questions should you ask an advocate, lawyer, doctor or support person about previous compensation or worsening?",
            HelperText = "Write down questions about previous decisions, missing letters, updated evidence, worsening history or appointment preparation.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 130,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Questions about previous compensation or worsening",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
    };
}
