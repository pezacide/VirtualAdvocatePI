using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class GarpMQuestionGroups
{
    public static IReadOnlyList<GarpMQuestionGroupTemplate> All { get; } = new List<GarpMQuestionGroupTemplate>
    {
        new()
        {
            GroupKey = "DIAGNOSIS_SYMPTOMS_TREATMENT",
            Title = "Diagnosis, symptoms and treatment",
            Description = "Capture the condition name, diagnosis status, current symptoms, treatment history, medication and side effects.",
            WhyThisMatters = "This helps organise the current clinical picture before speaking with a doctor, advocate, lawyer or support person.",
            DisplayOrder = 10,
            SafetyNote = GarpMSafetyBoundary.Text,
            Questions = DiagnosisSymptomsTreatmentQuestions.All,
        },
        new()
        {
            GroupKey = "STABILITY_TREATMENT_RESPONSE",
            Title = "Stability and treatment response",
            Description = "Capture whether the condition is stable, improving, worsening or fluctuating, and how treatment affects symptoms.",
            WhyThisMatters = "This helps identify whether the evidence describes the current state of the condition clearly enough.",
            DisplayOrder = 20,
            SafetyNote = GarpMSafetyBoundary.Text,
            Questions = StabilityTreatmentResponseQuestions.All,
        },
        new()
        {
            GroupKey = "FUNCTIONAL_LIFESTYLE_WORK_IMPACT",
            Title = "Functional, lifestyle and work impact",
            Description = "Capture how the condition affects daily activities, self-care, mobility, sleep, relationships, social participation and work.",
            WhyThisMatters = "This helps turn symptoms into practical examples of day-to-day impact without calculating impairment points.",
            DisplayOrder = 30,
            SafetyNote = GarpMSafetyBoundary.Text,
            Questions = FunctionalLifestyleWorkImpactQuestions.All,
        },
        new()
        {
            GroupKey = "WORSENING_PREVIOUS_COMPENSATION",
            Title = "Worsening and previous compensation",
            Description = "Capture previous DVA acceptance, previous compensation or assessment history, worsening since prior decisions, and available letters.",
            WhyThisMatters = "This helps organise background history where a condition may already have been accepted, assessed or compensated.",
            DisplayOrder = 40,
            SafetyNote = GarpMSafetyBoundary.Text,
            Questions = WorseningPreviousCompensationQuestions.All,
        },
        new()
        {
            GroupKey = "EVIDENCE_APPOINTMENT_PREP",
            Title = "Evidence gaps and appointment preparation",
            Description = "Capture what evidence exists, what is missing, what needs to be requested, and what questions should be asked at appointments.",
            WhyThisMatters = "This helps prepare for conversations with doctors, advocates, lawyers or support people.",
            DisplayOrder = 50,
            SafetyNote = GarpMSafetyBoundary.Text,
            Questions = EvidenceAppointmentPrepQuestions.All,
        },
    };

    public static IEnumerable<GarpMQuestionGroupTemplate> ActiveGroups => All.Where(g => g.Questions.Count > 0);
}
