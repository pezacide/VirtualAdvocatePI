namespace VirtualAdvocatePI.Mobile.Services.Dialogs;

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "Cancel")
    {
        return Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
    }
}
