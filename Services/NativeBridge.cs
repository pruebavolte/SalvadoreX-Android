using System.Runtime.InteropServices;
using Newtonsoft.Json;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid.Services;

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class NativeBridge
{
    private readonly Data.DatabaseService _db;
    private readonly SyncService _sync;
    private readonly LicensingService _licensing;
    
    public NativeBridge(Data.DatabaseService db, SyncService sync)
    {
        _db = db;
        _sync = sync;
        _licensing = new LicensingService();
    }
    
    public bool IsOffline => !_sync.IsOnline;
    
    public string GetProducts()
    {
        var products = _db.GetProductsAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(products);
    }
    
    public void SaveProduct(string json)
    {
        var product = JsonConvert.DeserializeObject<Product>(json);
        _db.SaveProductAsync(product).GetAwaiter().GetResult();
    }
    
    public string GetCustomers()
    {
        var customers = _db.GetCustomersAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(customers);
    }
    
    public void SaveCustomer(string json)
    {
        var customer = JsonConvert.DeserializeObject<Customer>(json);
        _db.SaveCustomerAsync(customer).GetAwaiter().GetResult();
    }
    
    public string GetSales()
    {
        var sales = _db.GetSalesAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(sales);
    }
    
    public void SaveSale(string json)
    {
        var sale = JsonConvert.DeserializeObject<Sale>(json);
        _db.SaveSaleAsync(sale, sale.Items).GetAwaiter().GetResult();
    }
    
    public string GetSetting(string key)
    {
        return _db.GetSettingAsync(key).GetAwaiter().GetResult();
    }
    
    public void SetSetting(string key, string value)
    {
        _db.SetSettingAsync(key, value).GetAwaiter().GetResult();
    }
    
    public void SyncNow()
    {
        _sync.ForceSyncNowAsync().GetAwaiter().GetResult();
    }
    
    public string GetHardwareId()
    {
        return _licensing.GetHardwareId();
    }
    
    public string GetCategories()
    {
        var categories = _db.GetCategoriesAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(categories);
    }
    
    public void SaveCategory(string json)
    {
        var category = JsonConvert.DeserializeObject<Category>(json);
        _db.SaveCategoryAsync(category).GetAwaiter().GetResult();
    }
}
