using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class AiDraftReviewPage : ContentPage
{
    public AiDraftReviewPage(AiDraftReviewViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
