using System.Collections.ObjectModel;
using System.Windows.Input;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.ViewModels
{
    public class POSViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        
        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<CartItem> CartItems { get; } = new();
        
        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                _ = LoadProductsAsync();
            }
        }

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

        public decimal CartTotal => CartItems.Sum(i => i.Total);
        public int CartCount => CartItems.Sum(i => i.Quantity);

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand RefreshCommand { get; }

        public POSViewModel(DatabaseService db)
        {
            _db = db;
            Title = "Punto de Venta";

            AddToCartCommand = new Command<Product>(AddToCart);
            RemoveFromCartCommand = new Command<CartItem>(RemoveFromCart);
            ClearCartCommand = new Command(ClearCart);
            CheckoutCommand = new Command(async () => await CheckoutAsync());
            RefreshCommand = new Command(async () => await LoadDataAsync());
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
            var products = await _db.GetProductsAsync(activeOnly: true);

            var filtered = products.Where(p => p.AvailablePos);

            if (SelectedCategory != null)
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);

            if (!string.IsNullOrEmpty(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.Barcode?.ToLower().Contains(search) ?? false));
            }

            foreach (var product in filtered)
                Products.Add(product);
        }

        private void AddToCart(Product product)
        {
            var existing = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity++;
                var index = CartItems.IndexOf(existing);
                CartItems.RemoveAt(index);
                CartItems.Insert(index, existing);
            }
            else
            {
                CartItems.Add(new CartItem { Product = product, Quantity = 1 });
            }
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(CartCount));
        }

        private void RemoveFromCart(CartItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
                var index = CartItems.IndexOf(item);
                CartItems.RemoveAt(index);
                CartItems.Insert(index, item);
            }
            else
            {
                CartItems.Remove(item);
            }
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(CartCount));
        }

        private void ClearCart()
        {
            CartItems.Clear();
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(CartCount));
        }

        private async Task CheckoutAsync()
        {
            if (!CartItems.Any())
            {
                await Shell.Current.DisplayAlert("Aviso", "Agregue productos al carrito", "OK");
                return;
            }

            var paymentMethods = new[] { "Efectivo", "Tarjeta", "Transferencia" };
            var method = await Shell.Current.DisplayActionSheet("Metodo de Pago", "Cancelar", null, paymentMethods);
            
            if (string.IsNullOrEmpty(method) || method == "Cancelar") return;

            IsBusy = true;

            try
            {
                var taxRate = decimal.Parse(await _db.GetSettingAsync("tax_rate", "16"));
                var subtotal = CartTotal;
                var tax = subtotal * (taxRate / 100);
                var total = subtotal + tax;

                var sale = new Sale
                {
                    ReceiptNumber = await _db.GenerateReceiptNumberAsync(),
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total,
                    PaymentMethod = method.ToLower(),
                    AmountPaid = total,
                    Status = "completed"
                };

                var items = CartItems.Select(ci => new SaleItem
                {
                    ProductId = ci.Product.Id,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price,
                    Total = ci.Total
                }).ToList();

                await _db.SaveSaleAsync(sale, items);

                // Update stock
                foreach (var item in CartItems)
                {
                    var product = item.Product;
                    product.Stock -= item.Quantity;
                    await _db.SaveProductAsync(product);
                }

                ClearCart();
                await Shell.Current.DisplayAlert("Venta Completada", 
                    $"Recibo: {sale.ReceiptNumber}\nTotal: ${total:N2}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
