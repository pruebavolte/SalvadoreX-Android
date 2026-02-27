using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Views;
using Android.Content.PM;
using Android.Runtime;
using Java.Interop;
using Newtonsoft.Json;
using SalvadoreXAndroid.Services;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Models;

namespace SalvadoreXAndroid;

[Activity(
    Label = "SalvadoreX POS",
    MainLauncher = true,
    Theme = "@android:style/Theme.Material.Light.NoActionBar",
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
    private Android.Webkit.WebView? _webView;
    private Data.DatabaseService? _db;
    private SyncService? _sync;
    private LicensingService? _licensing;
    
    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Fullscreen
        Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        
        // Initialize services
        _db = new DatabaseService();
        await _db.InitializeAsync();
        
        _sync = new SyncService(_db);
        _licensing = new LicensingService();
        
        // Verify license
        if (!_licensing.ValidateLicense())
        {
            ShowActivationDialog();
            return;
        }
        
        // Create WebView
        _webView = new Android.Webkit.WebView(this);
        _webView.Settings.JavaScriptEnabled = true;
        _webView.Settings.DomStorageEnabled = true;
        _webView.Settings.AllowFileAccess = true;
        _webView.Settings.AllowContentAccess = true;
        _webView.Settings.CacheMode = CacheModes.Normal;
        
        // JavaScript interface for native bridge
        var bridge = new NativeBridge(_db, _sync, _licensing);
        _webView.AddJavascriptInterface(bridge, "NativeAPI");
        
        // Load web app
        _webView.LoadUrl("file:///android_asset/webapp/index.html");
        
        SetContentView(_webView);
        
        // Start background sync
        _sync.StartBackgroundSync();
    }
    
    private void ShowActivationDialog()
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Activación Requerida");
        builder.SetMessage($"Esta copia no está activada.\n\nID de Hardware:\n{_licensing?.GetHardwareId()}\n\nContacte a su proveedor.");
        builder.SetPositiveButton("Cerrar", (s, e) => FinishAffinity());
        builder.SetCancelable(false);
        builder.Show();
    }
    
    public override void OnBackPressed()
    {
        if (_webView?.CanGoBack() == true)
        {
            _webView.GoBack();
        }
        else
        {
            base.OnBackPressed();
        }
    }
    
    protected override void OnDestroy()
    {
        _sync?.StopBackgroundSync();
        base.OnDestroy();
    }
}

// JavaScript Bridge
public class NativeBridge : Java.Lang.Object
{
    private readonly DatabaseService _db;
    private readonly SyncService _sync;
    private readonly LicensingService _licensing;
    
    public NativeBridge(Data.DatabaseService db, SyncService sync, LicensingService licensing)
    {
        _db = db;
        _sync = sync;
        _licensing = licensing;
    }
    
    [JavascriptInterface]
    [Export("isOffline")]
    public bool IsOffline() => !_sync.IsOnline;
    
    [JavascriptInterface]
    [Export("getProducts")]
    public string GetProducts() 
    {
        var products = _db.GetProductsAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(products);
    }
    
    [JavascriptInterface]
    [Export("saveProduct")]
    public void SaveProduct(string json) 
    {
        var product = JsonConvert.DeserializeObject<Product>(json);
        _db.SaveProductAsync(product).GetAwaiter().GetResult();
    }
    
    [JavascriptInterface]
    [Export("getCustomers")]
    public string GetCustomers() 
    {
        var customers = _db.GetCustomersAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(customers);
    }
    
    [JavascriptInterface]
    [Export("saveCustomer")]
    public void SaveCustomer(string json) 
    {
        var customer = JsonConvert.DeserializeObject<Customer>(json);
        _db.SaveCustomerAsync(customer).GetAwaiter().GetResult();
    }
    
    [JavascriptInterface]
    [Export("getSales")]
    public string GetSales() 
    {
        var sales = _db.GetSalesAsync().GetAwaiter().GetResult();
        return JsonConvert.SerializeObject(sales);
    }
    
    [JavascriptInterface]
    [Export("saveSale")]
    public void SaveSale(string json) 
    {
        var sale = JsonConvert.DeserializeObject<Sale>(json);
        _db.SaveSaleAsync(sale, sale.Items).GetAwaiter().GetResult();
    }
    
    [JavascriptInterface]
    [Export("getSetting")]
    public string GetSetting(string key) => _db.GetSettingAsync(key).GetAwaiter().GetResult();
    
    [JavascriptInterface]
    [Export("setSetting")]
    public void SetSetting(string key, string value) => _db.SetSettingAsync(key, value).GetAwaiter().GetResult();
    
    [JavascriptInterface]
    [Export("syncNow")]
    public void SyncNow() => _sync.ForceSyncNowAsync().GetAwaiter().GetResult();
    
    [JavascriptInterface]
    [Export("getHardwareId")]
    public string GetHardwareId() => _licensing.GetHardwareId();
}
