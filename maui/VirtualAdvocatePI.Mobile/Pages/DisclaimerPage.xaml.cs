using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class DisclaimerPage : ContentPage
{
    private readonly DisclaimerViewModel _viewModel;

    public DisclaimerPage(DisclaimerViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.CheckAcceptanceCommand.ExecuteAsync(null);
    }
}
