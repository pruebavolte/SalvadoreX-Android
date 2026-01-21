using SalvadoreXAndroid.ViewModels;

namespace SalvadoreXAndroid.Views;

public partial class POSPage : ContentPage
{
    private readonly POSViewModel _viewModel;
    
    public POSPage(POSViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }
}
