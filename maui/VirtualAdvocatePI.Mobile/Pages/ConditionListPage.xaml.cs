using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class ConditionListPage : ContentPage
{
    public ConditionListPage(ConditionListViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
