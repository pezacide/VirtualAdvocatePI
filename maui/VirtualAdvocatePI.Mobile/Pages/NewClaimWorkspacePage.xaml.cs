using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class NewClaimWorkspacePage : ContentPage
{
    public NewClaimWorkspacePage(NewClaimWorkspaceViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
