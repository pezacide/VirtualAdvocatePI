using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class FunctionalLifestyleWorkImpactQuestions
{
    private const string GroupKey = "FUNCTIONAL_LIFESTYLE_WORK_IMPACT";

    public static IReadOnlyList<GarpMQuestionTemplate> All { get; } = new List<GarpMQuestionTemplate>
    {
        new()
        {
            Id = "flw_daily_activity_impact",
            GroupKey = GroupKey,
            QuestionKey = "daily_activity_impact",
            QuestionText = "How does this condition affect ordinary daily activities?",
            HelperText = "Describe practical examples such as housework, shopping, cooking, driving, appointments, exercise, hobbies or managing routines.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 10,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Daily activity impact",
            ValidationRules = new List<GarpMValidationRule>
            {
                new() { Type = "MIN_LENGTH", Value = 10, Message = "Add at least one practical daily activity example." },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_impact_frequency",
            GroupKey = GroupKey,
            QuestionKey = "impact_frequency",
            QuestionText = "How often does this impact happen?",
            HelperText = "Choose the closest option. This helps organise whether the impact is daily, frequent, occasional or mainly during flare-ups.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 20,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Impact frequency",
            Options = GarpMOptionSets.ImpactFrequency,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_self_care_impact",
            GroupKey = GroupKey,
            QuestionKey = "self_care_impact",
            QuestionText = "Does the condition affect self-care or personal routines?",
            HelperText = "Examples include washing, dressing, grooming, preparing for the day, medication routines, eating, fatigue management or needing extra time.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 30,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Self-care impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_mobility_physical_impact",
            GroupKey = GroupKey,
            QuestionKey = "mobility_physical_impact",
            QuestionText = "Does the condition affect movement, physical tasks or getting around?",
            HelperText = "Describe walking, standing, sitting, lifting, bending, stairs, driving, public transport, balance, falls, pain, fatigue or other limits.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 40,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Mobility and physical impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_sleep_impact",
            GroupKey = GroupKey,
            QuestionKey = "sleep_impact",
            QuestionText = "Does the condition affect sleep or rest?",
            HelperText = "Describe trouble falling asleep, waking during the night, nightmares, pain, tinnitus, medication effects, fatigue or needing naps.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 50,
            EvidenceCategory = "LIFESTYLE_IMPACT",
            SummaryLabel = "Sleep impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_relationship_social_impact",
            GroupKey = GroupKey,
            QuestionKey = "relationship_social_impact",
            QuestionText = "Does the condition affect relationships, family life or social participation?",
            HelperText = "Describe impacts on family, friends, social events, communication, mood, irritability, avoidance, isolation or needing support.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 60,
            EvidenceCategory = "LIFESTYLE_IMPACT",
            SummaryLabel = "Relationship and social impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_work_impact",
            GroupKey = GroupKey,
            QuestionKey = "work_impact",
            QuestionText = "Does the condition affect work, study or volunteering?",
            HelperText = "Describe missed work, reduced hours, modified duties, concentration, physical limits, fatigue, pain, stress, performance, reliability or needing support.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 70,
            EvidenceCategory = "WORK_IMPACT",
            SummaryLabel = "Work, study or volunteering impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_work_adjustments",
            GroupKey = GroupKey,
            QuestionKey = "work_adjustments",
            QuestionText = "Have you needed changes or adjustments at work, study or volunteering?",
            HelperText = "Examples include changed duties, extra breaks, reduced hours, working from home, avoiding tasks, using aids, taking leave or stopping work.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 80,
            EvidenceCategory = "WORK_IMPACT",
            SummaryLabel = "Work adjustments",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_support_needed",
            GroupKey = GroupKey,
            QuestionKey = "support_needed",
            QuestionText = "Do you need help from another person, aids, reminders or routines because of this condition?",
            HelperText = "Describe help from family, friends, carers, colleagues, reminders, apps, mobility aids, hearing aids, braces, equipment or routines.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 90,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Support needed",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_good_day_bad_day",
            GroupKey = GroupKey,
            QuestionKey = "good_day_bad_day",
            QuestionText = "What is the difference between a better day and a worse day?",
            HelperText = "This helps capture variation without trying to calculate a score. Give practical examples of what changes.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 100,
            EvidenceCategory = "FUNCTIONAL_IMPACT",
            SummaryLabel = "Better day and worse day examples",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_impact_evidence_available",
            GroupKey = GroupKey,
            QuestionKey = "impact_evidence_available",
            QuestionText = "What evidence or notes show this functional, lifestyle or work impact?",
            HelperText = "Examples include personal notes, doctor reports, allied health records, employer records, leave records, support letters or appointment notes.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 110,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Impact evidence available",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "flw_impact_questions_for_doctor",
            GroupKey = GroupKey,
            QuestionKey = "impact_questions_for_doctor",
            QuestionText = "What questions should you ask your doctor about functional, lifestyle or work impact?",
            HelperText = "Write down anything you want your doctor to clarify, confirm or document at the next appointment.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 120,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Doctor questions about impact",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
    };
}
