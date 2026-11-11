namespace VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;

/// <summary>
/// Shared shape for the Claim Starter Pack and Doctor Guidance Pack generate
/// endpoints. Both return a document plus a set of counts; unused fields stay
/// null for the pack that does not supply them.
/// </summary>
public sealed class GeneratePackResponse
{
    public GeneratedDocument? Document { get; init; }

    public bool Generated { get; init; }

    public string? DocumentVersion { get; init; }

    public int ActiveConditionCount { get; init; }

    public int EvidenceItemCount { get; init; }

    public int EvidenceGapCount { get; init; }

    public int IncludedAiDraftCount { get; init; }

    public int ExcludedUnapprovedAiDraftCount { get; init; }

    public int IncludedApprovedDoctorDraftCount { get; init; }

    public int ExcludedUnapprovedDoctorDraftCount { get; init; }

    public string? ReviewedOnlyRule { get; init; }
}
