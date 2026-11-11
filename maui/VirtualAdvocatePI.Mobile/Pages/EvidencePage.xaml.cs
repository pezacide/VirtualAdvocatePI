using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class EvidencePage : ContentPage
{
    public EvidencePage(EvidenceViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
