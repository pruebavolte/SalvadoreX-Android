using System.Windows.Input;
using SalvadoreXAndroid.Data;
using SalvadoreXAndroid.Services;

namespace SalvadoreXAndroid.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly SyncService _sync;

        private string _businessName = string.Empty;
        public string BusinessName
        {
            get => _businessName;
            set => SetProperty(ref _businessName, value);
        }

        private string _businessPhone = string.Empty;
        public string BusinessPhone
        {
            get => _businessPhone;
            set => SetProperty(ref _businessPhone, value);
        }

        private string _taxRate = "16";
        public string TaxRate
        {
            get => _taxRate;
            set => SetProperty(ref _taxRate, value);
        }

        private string _syncStatus = "Verificando...";
        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
        }

        private bool _isOnline;
        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand SyncNowCommand { get; }

        public SettingsViewModel(DatabaseService db, SyncService sync)
        {
            _db = db;
            _sync = sync;
            Title = "Configuracion";

            SaveCommand = new Command(async () => await SaveSettingsAsync());
            SyncNowCommand = new Command(async () => await SyncNowAsync());

            _sync.StatusChanged += (s, msg) => SyncStatus = msg;
        }

        public async Task LoadSettingsAsync()
        {
            BusinessName = await _db.GetSettingAsync("business_name", "Mi Negocio");
            BusinessPhone = await _db.GetSettingAsync("business_phone", "");
            TaxRate = await _db.GetSettingAsync("tax_rate", "16");
            
            IsOnline = _sync.IsOnline;
            SyncStatus = IsOnline ? "Conectado" : "Sin conexion";
        }

        private async Task SaveSettingsAsync()
        {
            IsBusy = true;

            try
            {
                await _db.SetSettingAsync("business_name", BusinessName);
                await _db.SetSettingAsync("business_phone", BusinessPhone);
                await _db.SetSettingAsync("tax_rate", TaxRate);

                await Shell.Current.DisplayAlert("Exito", "Configuracion guardada", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SyncNowAsync()
        {
            IsBusy = true;
            SyncStatus = "Sincronizando...";

            try
            {
                await _sync.ForceSyncNowAsync();
                SyncStatus = _sync.IsOnline ? $"Sincronizado: {DateTime.Now:HH:mm}" : "Sin conexion";
            }
            catch (Exception ex)
            {
                SyncStatus = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
