using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class GarpMStructuredSummaryPage : ContentPage
{
    public GarpMStructuredSummaryPage(GarpMStructuredSummaryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
