using System.Collections.ObjectModel;
using System.Windows.Input;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.ViewModels
{
    public class CustomersViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Customer> Customers { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = LoadCustomersAsync();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public CustomersViewModel(DatabaseService db)
        {
            _db = db;
            Title = "Clientes";

            RefreshCommand = new Command(async () => await LoadCustomersAsync());
            AddCustomerCommand = new Command(async () => await AddCustomerAsync());
            EditCustomerCommand = new Command<Customer>(async (c) => await EditCustomerAsync(c));
            DeleteCustomerCommand = new Command<Customer>(async (c) => await DeleteCustomerAsync(c));
        }

        public async Task LoadCustomersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Customers.Clear();
                var customers = await _db.GetCustomersAsync(activeOnly: false);

                if (!string.IsNullOrEmpty(SearchText))
                {
                    var search = SearchText.ToLower();
                    customers = customers.Where(c =>
                        c.Name.ToLower().Contains(search) ||
                        (c.Email?.ToLower().Contains(search) ?? false) ||
                        (c.Phone?.ToLower().Contains(search) ?? false)).ToList();
                }

                foreach (var customer in customers)
                    Customers.Add(customer);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddCustomerAsync()
        {
            var name = await Shell.Current.DisplayPromptAsync("Nuevo Cliente", "Nombre:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var phone = await Shell.Current.DisplayPromptAsync("Nuevo Cliente", "Telefono:", keyboard: Keyboard.Telephone);
            var email = await Shell.Current.DisplayPromptAsync("Nuevo Cliente", "Email:", keyboard: Keyboard.Email);

            var customer = new Customer
            {
                Name = name,
                Phone = phone,
                Email = email
            };

            await _db.SaveCustomerAsync(customer);
            await LoadCustomersAsync();
        }

        private async Task EditCustomerAsync(Customer customer)
        {
            var name = await Shell.Current.DisplayPromptAsync("Editar Cliente", "Nombre:", initialValue: customer.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            var phone = await Shell.Current.DisplayPromptAsync("Editar Cliente", "Telefono:", 
                initialValue: customer.Phone, keyboard: Keyboard.Telephone);
            var email = await Shell.Current.DisplayPromptAsync("Editar Cliente", "Email:", 
                initialValue: customer.Email, keyboard: Keyboard.Email);

            customer.Name = name;
            customer.Phone = phone;
            customer.Email = email;

            await _db.SaveCustomerAsync(customer);
            await LoadCustomersAsync();
        }

        private async Task DeleteCustomerAsync(Customer customer)
        {
            var confirm = await Shell.Current.DisplayAlert("Confirmar", 
                $"Eliminar cliente '{customer.Name}'?", "Si", "No");
            
            if (confirm)
            {
                await _db.DeleteCustomerAsync(customer.Id);
                await LoadCustomersAsync();
            }
        }
    }
}
