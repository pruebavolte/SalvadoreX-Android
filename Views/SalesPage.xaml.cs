using SalvadoreXAndroid.ViewModels;

namespace SalvadoreXAndroid.Views;

public partial class SalesPage : ContentPage
{
    private readonly SalesViewModel _viewModel;
    
    public SalesPage(SalesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSalesAsync();
    }
}
