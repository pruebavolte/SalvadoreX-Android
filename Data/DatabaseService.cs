using SQLite;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        
        public async Task InitializeAsync()
        {
            if (_database != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "salvadorex.db3");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<Product>();
            await _database.CreateTableAsync<Category>();
            await _database.CreateTableAsync<Customer>();
            await _database.CreateTableAsync<Sale>();
            await _database.CreateTableAsync<SaleItem>();
            await _database.CreateTableAsync<Setting>();
            
            await SeedDefaultDataAsync();
        }

        private async Task SeedDefaultDataAsync()
        {
            var categoryCount = await _database!.Table<Category>().CountAsync();
            if (categoryCount == 0)
            {
                var defaultCategories = new[]
                {
                    new Category { Name = "Bebidas", Description = "Refrescos, jugos, agua" },
                    new Category { Name = "Alimentos", Description = "Comida preparada" },
                    new Category { Name = "Snacks", Description = "Botanas y dulces" },
                    new Category { Name = "General", Description = "Productos generales" }
                };

                foreach (var cat in defaultCategories)
                {
                    await _database.InsertAsync(cat);
                }
            }

            var settingsCount = await _database.Table<Setting>().CountAsync();
            if (settingsCount == 0)
            {
                var defaultSettings = new[]
                {
                    new Setting { Key = "business_name", Value = "Mi Negocio" },
                    new Setting { Key = "tax_rate", Value = "16" },
                    new Setting { Key = "receipt_counter", Value = "1" }
                };

                foreach (var setting in defaultSettings)
                {
                    await _database.InsertAsync(setting);
                }
            }
        }

        // Products
        public Task<List<Product>> GetProductsAsync(bool activeOnly = true)
        {
            if (activeOnly)
                return _database!.Table<Product>().Where(p => p.Active).ToListAsync();
            return _database!.Table<Product>().ToListAsync();
        }

        public Task<Product?> GetProductAsync(string id) => 
            _database!.Table<Product>().Where(p => p.Id == id).FirstOrDefaultAsync();

        public Task<int> SaveProductAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow.ToString("o");
            product.NeedSync = true;
            return _database!.InsertOrReplaceAsync(product);
        }

        public Task<int> DeleteProductAsync(string id) =>
            _database!.ExecuteAsync("UPDATE products SET active = 0, need_sync = 1, updated_at = ? WHERE id = ?",
                DateTime.UtcNow.ToString("o"), id);

        // Categories
        public Task<List<Category>> GetCategoriesAsync() =>
            _database!.Table<Category>().Where(c => c.Active).ToListAsync();

        public Task<int> SaveCategoryAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow.ToString("o");
            category.NeedSync = true;
            return _database!.InsertOrReplaceAsync(category);
        }

        // Customers
        public Task<List<Customer>> GetCustomersAsync(bool activeOnly = true)
        {
            if (activeOnly)
                return _database!.Table<Customer>().Where(c => c.Active).ToListAsync();
            return _database!.Table<Customer>().ToListAsync();
        }

        public Task<int> SaveCustomerAsync(Customer customer)
        {
            customer.UpdatedAt = DateTime.UtcNow.ToString("o");
            customer.NeedSync = true;
            return _database!.InsertOrReplaceAsync(customer);
        }

        public Task<int> DeleteCustomerAsync(string id) =>
            _database!.ExecuteAsync("UPDATE customers SET active = 0, need_sync = 1, updated_at = ? WHERE id = ?",
                DateTime.UtcNow.ToString("o"), id);

        // Sales
        public Task<List<Sale>> GetSalesAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _database!.Table<Sale>().OrderByDescending(s => s.CreatedAt);
            return query.ToListAsync();
        }

        public async Task<Sale?> GetSaleWithItemsAsync(string id)
        {
            var sale = await _database!.Table<Sale>().Where(s => s.Id == id).FirstOrDefaultAsync();
            if (sale != null)
            {
                sale.Items = await _database.Table<SaleItem>().Where(i => i.SaleId == id).ToListAsync();
            }
            return sale;
        }

        public async Task<int> SaveSaleAsync(Sale sale, List<SaleItem> items)
        {
            await _database!.InsertAsync(sale);
            foreach (var item in items)
            {
                item.SaleId = sale.Id;
                await _database.InsertAsync(item);
            }
            return 1;
        }

        // Settings
        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            var setting = await _database!.Table<Setting>().Where(s => s.Key == key).FirstOrDefaultAsync();
            return setting?.Value ?? defaultValue;
        }

        public Task SetSettingAsync(string key, string value)
        {
            return _database!.InsertOrReplaceAsync(new Setting { Key = key, Value = value });
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var counter = await GetSettingAsync("receipt_counter", "1");
            var number = int.Parse(counter);
            var receiptNumber = $"REC-{DateTime.Now:yyyyMMdd}-{number:D4}";
            await SetSettingAsync("receipt_counter", (number + 1).ToString());
            return receiptNumber;
        }

        // Sync
        public Task<List<Product>> GetPendingSyncProductsAsync() =>
            _database!.Table<Product>().Where(p => p.NeedSync).ToListAsync();

        public Task<List<Customer>> GetPendingSyncCustomersAsync() =>
            _database!.Table<Customer>().Where(c => c.NeedSync).ToListAsync();

        public Task<List<Sale>> GetPendingSyncSalesAsync() =>
            _database!.Table<Sale>().Where(s => s.NeedSync).ToListAsync();

        public Task MarkProductSyncedAsync(string id) =>
            _database!.ExecuteAsync("UPDATE products SET need_sync = 0 WHERE id = ?", id);

        public Task MarkCustomerSyncedAsync(string id) =>
            _database!.ExecuteAsync("UPDATE customers SET need_sync = 0 WHERE id = ?", id);

        public Task MarkSaleSyncedAsync(string id) =>
            _database!.ExecuteAsync("UPDATE sales SET need_sync = 0 WHERE id = ?", id);
    }
}
