using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class GarpMQuestionEnginePage : ContentPage
{
    public GarpMQuestionEnginePage(GarpMQuestionEngineViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
