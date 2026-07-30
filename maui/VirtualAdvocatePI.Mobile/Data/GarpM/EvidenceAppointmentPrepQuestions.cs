using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class EvidenceAppointmentPrepQuestions
{
    private const string GroupKey = "EVIDENCE_APPOINTMENT_PREP";

    private static readonly IReadOnlyList<GarpMQuestionOption> EvidenceCategoryOptions = new List<GarpMQuestionOption>
    {
        new() { Value = "DIAGNOSIS", Label = "Diagnosis evidence" },
        new() { Value = "SYMPTOMS", Label = "Symptoms evidence" },
        new() { Value = "TREATMENT", Label = "Treatment evidence" },
        new() { Value = "MEDICATION", Label = "Medication evidence" },
        new() { Value = "FUNCTIONAL_IMPACT", Label = "Functional impact evidence" },
        new() { Value = "LIFESTYLE_IMPACT", Label = "Lifestyle impact evidence" },
        new() { Value = "WORK_IMPACT", Label = "Work impact evidence" },
        new() { Value = "WORSENING", Label = "Worsening evidence" },
        new() { Value = "PREVIOUS_COMPENSATION", Label = "Previous DVA or compensation evidence" },
        new() { Value = "SERVICE_CONNECTION", Label = "Service connection notes" },
        new() { Value = "APPOINTMENT_PREP", Label = "Appointment preparation notes" },
    };

    public static IReadOnlyList<GarpMQuestionTemplate> All { get; } = new List<GarpMQuestionTemplate>
    {
        new()
        {
            Id = "eap_existing_evidence_summary",
            GroupKey = GroupKey,
            QuestionKey = "existing_evidence_summary",
            QuestionText = "What evidence do you already have for this condition?",
            HelperText = "List documents, reports, notes or records you already have. Examples include GP reports, specialist reports, DVA letters, treatment notes, medication lists, imaging, hearing tests, personal notes or support letters.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Required,
            DisplayOrder = 10,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Existing evidence",
            ValidationRules = new List<GarpMValidationRule>
            {
                new() { Type = "MIN_LENGTH", Value = 5, Message = "Add a short description of existing evidence, or write that you do not have any yet." },
            },
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_missing_evidence_summary",
            GroupKey = GroupKey,
            QuestionKey = "missing_evidence_summary",
            QuestionText = "What evidence do you think is still missing?",
            HelperText = "List anything that may need to be requested, located or discussed. This is a preparation prompt only and does not say what DVA will require.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 20,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Missing evidence",
            SafetyNote = "This app does not decide what evidence DVA will require. It only helps organise possible gaps for discussion with a doctor, advocate, lawyer or support person.",
        },
        new()
        {
            Id = "eap_evidence_categories_missing",
            GroupKey = GroupKey,
            QuestionKey = "evidence_categories_missing",
            QuestionText = "Which evidence categories may need more work?",
            HelperText = "Tick any areas that may need more information, documents or discussion.",
            AnswerType = GarpMAnswerTypes.MultiSelect,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 30,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Evidence categories needing work",
            Options = EvidenceCategoryOptions,
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_documents_to_request",
            GroupKey = GroupKey,
            QuestionKey = "documents_to_request",
            QuestionText = "What documents do you need to request or locate?",
            HelperText = "Examples include GP records, specialist reports, DVA decision letters, previous assessment letters, medication history, treatment plans, imaging, test results or employer records.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 40,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Documents to request",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_document_sources",
            GroupKey = GroupKey,
            QuestionKey = "document_sources",
            QuestionText = "Who might hold the missing documents?",
            HelperText = "Examples include a GP clinic, specialist, hospital, allied health provider, employer, DVA, Open Arms, service records, or personal files.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 50,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Possible document sources",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_doctor_report_needed",
            GroupKey = GroupKey,
            QuestionKey = "doctor_report_needed",
            QuestionText = "Do you need to ask a doctor for a report, summary or clarification?",
            HelperText = "Choose the closest answer. This can help prepare appointment questions.",
            AnswerType = GarpMAnswerTypes.YesNoUnsure,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 60,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Doctor report or clarification needed",
            Options = GarpMOptionSets.YesNoUnsure,
            SafetyNote = "This app does not tell a doctor what to write. It helps the user prepare questions and organise information for discussion.",
        },
        new()
        {
            Id = "eap_questions_for_doctor",
            GroupKey = GroupKey,
            QuestionKey = "questions_for_doctor",
            QuestionText = "What questions should you ask your doctor at the next appointment?",
            HelperText = "Examples include diagnosis confirmation, current symptoms, treatment history, medication side effects, stability, functional impact, worsening, or whether records can be provided.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Recommended,
            DisplayOrder = 70,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Questions for doctor",
            SafetyNote = "This is appointment preparation only. It does not provide medical advice or replace a clinical assessment.",
        },
        new()
        {
            Id = "eap_questions_for_advocate_or_lawyer",
            GroupKey = GroupKey,
            QuestionKey = "questions_for_advocate_or_lawyer",
            QuestionText = "What questions should you ask an advocate, lawyer or support person?",
            HelperText = "Examples include pathway questions, missing DVA letters, previous decisions, evidence organisation, timeframes, forms or next steps.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 80,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Questions for advocate, lawyer or support person",
            SafetyNote = "This app does not provide legal advice. Use this prompt to prepare questions for a qualified person or support service.",
        },
        new()
        {
            Id = "eap_appointment_notes",
            GroupKey = GroupKey,
            QuestionKey = "appointment_notes",
            QuestionText = "What notes do you want to take into the appointment?",
            HelperText = "Write short reminders about symptoms, treatment, impact, worsening, missing evidence or documents to request.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 90,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Appointment notes",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_follow_up_actions",
            GroupKey = GroupKey,
            QuestionKey = "follow_up_actions",
            QuestionText = "What follow-up actions should happen after the appointment?",
            HelperText = "Examples include requesting records, uploading evidence, booking another appointment, asking for a report, updating notes, or speaking with an advocate or lawyer.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 100,
            EvidenceCategory = "APPOINTMENT_PREP",
            SummaryLabel = "Follow-up actions",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_upload_priority",
            GroupKey = GroupKey,
            QuestionKey = "upload_priority",
            QuestionText = "Which evidence should be uploaded or organised first?",
            HelperText = "List the highest priority items. This helps prepare the evidence metadata and upload steps.",
            AnswerType = GarpMAnswerTypes.LongText,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 110,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "Evidence upload priority",
            SafetyNote = GarpMSafetyBoundary.Text,
        },
        new()
        {
            Id = "eap_user_confidence",
            GroupKey = GroupKey,
            QuestionKey = "user_confidence",
            QuestionText = "How confident are you that the evidence picture is clear?",
            HelperText = "This is not a legal or medical assessment. It helps identify whether the user feels ready or needs more help organising information.",
            AnswerType = GarpMAnswerTypes.SingleSelect,
            RequirementLevel = GarpMRequirementLevels.Optional,
            DisplayOrder = 120,
            EvidenceCategory = "EVIDENCE_GAP",
            SummaryLabel = "User confidence in evidence picture",
            Options = new List<GarpMQuestionOption>
            {
                new() { Value = "CONFIDENT", Label = "Confident" },
                new() { Value = "SOMEWHAT_CONFIDENT", Label = "Somewhat confident" },
                new() { Value = "NOT_CONFIDENT", Label = "Not confident" },
                new() { Value = "UNSURE", Label = "Unsure" },
            },
            SafetyNote = "This confidence prompt is for preparation only. It does not predict a DVA outcome.",
        },
    };
}
