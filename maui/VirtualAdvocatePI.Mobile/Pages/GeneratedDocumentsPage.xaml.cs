using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class GeneratedDocumentsPage : ContentPage
{
    public GeneratedDocumentsPage(GeneratedDocumentsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
