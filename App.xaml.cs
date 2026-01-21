using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Services;

namespace SalvadoreXAndroid;

public partial class App : Application
{
    public App(DatabaseService databaseService, SyncService syncService)
    {
        InitializeComponent();
        
        // Initialize database
        databaseService.InitializeAsync().Wait();
        
        // Start background sync
        syncService.StartBackgroundSync();

        MainPage = new AppShell();
    }
}
