using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public partial class EvidenceChecklistItemState : ObservableObject
{
    public required string Label { get; init; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    /// <summary>Invoked after <see cref="IsChecked"/> changes so the owner can persist tick state.</summary>
    public Action? ChangedCallback { get; set; }

    partial void OnIsCheckedChanged(bool value) => ChangedCallback?.Invoke();
}
