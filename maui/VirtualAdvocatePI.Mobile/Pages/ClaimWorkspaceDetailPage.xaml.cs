using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class ClaimWorkspaceDetailPage : ContentPage
{
    public ClaimWorkspaceDetailPage(ClaimWorkspaceDetailViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
