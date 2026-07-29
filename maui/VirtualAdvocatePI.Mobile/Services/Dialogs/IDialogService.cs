namespace VirtualAdvocatePI.Mobile.Services.Dialogs;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "Cancel");
}
