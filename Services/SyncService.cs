using System.Net.Http.Json;
using Newtonsoft.Json;
using SalvadoreXAndroid.Data;

namespace SalvadoreXAndroid.Services
{
    public class SyncService
    {
        private readonly DatabaseService _db;
        private CancellationTokenSource? _cts;
        private readonly int _syncIntervalSeconds = 30;
        
        public bool IsOnline { get; private set; }
        public bool IsSyncing { get; private set; }
        public DateTime? LastSyncTime { get; private set; }
        
        public event EventHandler<string>? StatusChanged;

        public SyncService(DatabaseService db)
        {
            _db = db;
        }

        public void StartBackgroundSync()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => BackgroundSyncLoop(_cts.Token));
        }

        public void StopBackgroundSync()
        {
            _cts?.Cancel();
        }

        private async Task BackgroundSyncLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    IsOnline = await CheckInternetAsync();
                    
                    if (IsOnline)
                    {
                        await SyncPendingChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke(this, $"Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(_syncIntervalSeconds), cancellationToken);
            }
        }

        private async Task<bool> CheckInternetAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://www.google.com/generate_204");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task SyncPendingChangesAsync()
        {
            if (IsSyncing || !IsOnline) return;
            
            IsSyncing = true;
            StatusChanged?.Invoke(this, "Sincronizando...");

            try
            {
                var supabaseUrl = await _db.GetSettingAsync("supabase_url");
                var supabaseKey = await _db.GetSettingAsync("supabase_key");
                
                if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                {
                    StatusChanged?.Invoke(this, "Configuracion no disponible");
                    return;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                // Sync products
                var pendingProducts = await _db.GetPendingSyncProductsAsync();
                foreach (var product in pendingProducts)
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(product);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        await client.PostAsync($"{supabaseUrl}/rest/v1/products", content);
                        await _db.MarkProductSyncedAsync(product.Id);
                    }
                    catch { }
                }

                // Sync customers
                var pendingCustomers = await _db.GetPendingSyncCustomersAsync();
                foreach (var customer in pendingCustomers)
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(customer);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        await client.PostAsync($"{supabaseUrl}/rest/v1/customers", content);
                        await _db.MarkCustomerSyncedAsync(customer.Id);
                    }
                    catch { }
                }

                // Sync sales
                var pendingSales = await _db.GetPendingSyncSalesAsync();
                foreach (var sale in pendingSales)
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(sale);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        await client.PostAsync($"{supabaseUrl}/rest/v1/sales", content);
                        await _db.MarkSaleSyncedAsync(sale.Id);
                    }
                    catch { }
                }

                LastSyncTime = DateTime.Now;
                StatusChanged?.Invoke(this, $"Sincronizado: {LastSyncTime:HH:mm}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            }
            finally
            {
                IsSyncing = false;
            }
        }

        public Task ForceSyncNowAsync() => SyncPendingChangesAsync();
    }
}
