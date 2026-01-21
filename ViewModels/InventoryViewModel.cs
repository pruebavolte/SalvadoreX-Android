using System.Collections.ObjectModel;
using System.Windows.Input;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = LoadProductsAsync();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public InventoryViewModel(DatabaseService db)
        {
            _db = db;
            Title = "Inventario";

            RefreshCommand = new Command(async () => await LoadDataAsync());
            AddProductCommand = new Command(async () => await AddProductAsync());
            EditProductCommand = new Command<Product>(async (p) => await EditProductAsync(p));
            DeleteProductCommand = new Command<Product>(async (p) => await DeleteProductAsync(p));
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Categories.Clear();
                var categories = await _db.GetCategoriesAsync();
                foreach (var cat in categories)
                    Categories.Add(cat);

                await LoadProductsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var products = await _db.GetProductsAsync(activeOnly: false);

            if (!string.IsNullOrEmpty(SearchText))
            {
                var search = SearchText.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.Sku?.ToLower().Contains(search) ?? false) ||
                    (p.Barcode?.ToLower().Contains(search) ?? false)).ToList();
            }

            foreach (var product in products)
                Products.Add(product);
        }

        private async Task AddProductAsync()
        {
            var name = await Shell.Current.DisplayPromptAsync("Nuevo Producto", "Nombre:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var priceStr = await Shell.Current.DisplayPromptAsync("Nuevo Producto", "Precio:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(priceStr, out var price)) return;

            var product = new Product
            {
                Name = name,
                Price = price,
                Stock = 0
            };

            await _db.SaveProductAsync(product);
            await LoadProductsAsync();
        }

        private async Task EditProductAsync(Product product)
        {
            var name = await Shell.Current.DisplayPromptAsync("Editar Producto", "Nombre:", initialValue: product.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            var priceStr = await Shell.Current.DisplayPromptAsync("Editar Producto", "Precio:", 
                initialValue: product.Price.ToString(), keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(priceStr, out var price)) return;

            var stockStr = await Shell.Current.DisplayPromptAsync("Editar Producto", "Stock:", 
                initialValue: product.Stock.ToString(), keyboard: Keyboard.Numeric);
            if (!int.TryParse(stockStr, out var stock)) return;

            product.Name = name;
            product.Price = price;
            product.Stock = stock;

            await _db.SaveProductAsync(product);
            await LoadProductsAsync();
        }

        private async Task DeleteProductAsync(Product product)
        {
            var confirm = await Shell.Current.DisplayAlert("Confirmar", 
                $"Eliminar producto '{product.Name}'?", "Si", "No");
            
            if (confirm)
            {
                await _db.DeleteProductAsync(product.Id);
                await LoadProductsAsync();
            }
        }
    }
}
