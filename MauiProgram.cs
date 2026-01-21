using Microsoft.Extensions.Logging;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Services;
using SalvadoreXAndroid.Views;

namespace SalvadoreXAndroid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<SyncService>();
        
        // Register ViewModels
        builder.Services.AddSingleton<POSViewModel>();
        builder.Services.AddSingleton<InventoryViewModel>();
        builder.Services.AddSingleton<CustomersViewModel>();
        builder.Services.AddSingleton<SalesViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        
        // Register Views
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<POSPage>();
        builder.Services.AddSingleton<InventoryPage>();
        builder.Services.AddSingleton<CustomersPage>();
        builder.Services.AddSingleton<SalesPage>();
        builder.Services.AddSingleton<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
