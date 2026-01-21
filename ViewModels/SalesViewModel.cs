using System.Collections.ObjectModel;
using System.Windows.Input;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.ViewModels
{
    public class SalesViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Sale> Sales { get; } = new();

        public decimal TotalSales => Sales.Where(s => s.Status == "completed").Sum(s => s.Total);
        public int SalesCount => Sales.Count(s => s.Status == "completed");

        public ICommand RefreshCommand { get; }
        public ICommand ViewSaleCommand { get; }

        public SalesViewModel(DatabaseService db)
        {
            _db = db;
            Title = "Historial de Ventas";

            RefreshCommand = new Command(async () => await LoadSalesAsync());
            ViewSaleCommand = new Command<Sale>(async (s) => await ViewSaleAsync(s));
        }

        public async Task LoadSalesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Sales.Clear();
                var sales = await _db.GetSalesAsync();

                foreach (var sale in sales.OrderByDescending(s => s.CreatedAt))
                    Sales.Add(sale);

                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(SalesCount));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ViewSaleAsync(Sale sale)
        {
            var fullSale = await _db.GetSaleWithItemsAsync(sale.Id);
            if (fullSale == null) return;

            var details = $"Recibo: {fullSale.ReceiptNumber}\n" +
                         $"Fecha: {DateTime.Parse(fullSale.CreatedAt):dd/MM/yyyy HH:mm}\n\n" +
                         "Items:\n";

            foreach (var item in fullSale.Items)
            {
                details += $"  {item.Quantity}x {item.ProductName} = ${item.Total:N2}\n";
            }

            details += $"\nSubtotal: ${fullSale.Subtotal:N2}\n" +
                      $"IVA: ${fullSale.Tax:N2}\n" +
                      $"Total: ${fullSale.Total:N2}\n" +
                      $"Pago: {fullSale.PaymentMethod}";

            await Shell.Current.DisplayAlert("Detalle de Venta", details, "OK");
        }
    }
}
