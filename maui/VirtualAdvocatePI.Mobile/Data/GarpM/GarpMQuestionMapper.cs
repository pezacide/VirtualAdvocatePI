using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class GarpMQuestionMapper
{
    public static string ToBackendQuestionGroup(GarpMQuestionTemplate question)
    {
        return question.EvidenceCategory switch
        {
            "DIAGNOSIS" => "DIAGNOSIS",
            "SYMPTOMS" => "SYMPTOMS",
            "TREATMENT" => "TREATMENT",
            "MEDICATION" => "MEDICATION",
            "STABILITY" => "STABILITY",
            "FUNCTIONAL_IMPACT" => "FUNCTIONAL_IMPACT",
            "LIFESTYLE_IMPACT" => "LIFESTYLE_IMPACT",
            "WORK_IMPACT" => "WORK_IMPACT",
            "WORSENING" => "WORSENING",
            "PREVIOUS_COMPENSATION" => "PREVIOUS_COMPENSATION",
            "EVIDENCE_GAP" => "EVIDENCE_MISSING",
            "APPOINTMENT_PREP" => "CLAIM_CONTEXT",
            "SERVICE_CONNECTION" => "CLAIM_CONTEXT",
            "SUMMARY" => "CLAIM_CONTEXT",
            _ => "CLAIM_CONTEXT",
        };
    }
}
