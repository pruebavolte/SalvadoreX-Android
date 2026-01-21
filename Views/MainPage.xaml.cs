namespace SalvadoreXAndroid.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnPOSClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//POSPage");
    }

    private async void OnInventoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//InventoryPage");
    }

    private async void OnCustomersClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CustomersPage");
    }

    private async void OnSalesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SalesPage");
    }
}
