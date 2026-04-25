using System.Collections.ObjectModel;
using System.Text.Json;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;
using System.Windows.Input;

namespace ParfumAdmin_WPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        public ObservableCollection<TopProduct> TopProducts { get; } = new();

        private int _todayPageviews;
        public int TodayPageviews
        {
            get => _todayPageviews;
            set => SetProperty(ref _todayPageviews, value);
        }

        private int _todaySessions;
        public int TodaySessions
        {
            get => _todaySessions;
            set => SetProperty(ref _todaySessions, value);
        }

        // Egyedi látogatók ma: IP + eszköz-típus fingerprint alapján
        // (backend `unique_visitors` mező). Ha ugyanaz a gép kétszer kinyit
        // egy tabot, az egy látogató; ha mobilról is ránéz, az kettő.
        private int _todayUniqueVisitors;
        public int TodayUniqueVisitors
        {
            get => _todayUniqueVisitors;
            set => SetProperty(ref _todayUniqueVisitors, value);
        }

        private int _activeSessions;
        public int ActiveSessions
        {
            get => _activeSessions;
            set => SetProperty(ref _activeSessions, value);
        }

        private int _weekPageviews;
        public int WeekPageviews
        {
            get => _weekPageviews;
            set => SetProperty(ref _weekPageviews, value);
        }

        private int _weekNewVisitors;
        public int WeekNewVisitors
        {
            get => _weekNewVisitors;
            set => SetProperty(ref _weekNewVisitors, value);
        }

        private int _weekUniqueVisitors;
        public int WeekUniqueVisitors
        {
            get => _weekUniqueVisitors;
            set => SetProperty(ref _weekUniqueVisitors, value);
        }

        private int _monthPageviews;
        public int MonthPageviews
        {
            get => _monthPageviews;
            set => SetProperty(ref _monthPageviews, value);
        }

        private int _monthUniqueVisitors;
        public int MonthUniqueVisitors
        {
            get => _monthUniqueVisitors;
            set => SetProperty(ref _monthUniqueVisitors, value);
        }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(IApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // Három független végpont — párhuzamosan hívjuk őket, hogy a teljes
                // betöltés a leglassabb kérés idejébe kerüljön, ne az összeg időbe.
                await Task.WhenAll(
                    LoadOverviewAsync(),
                    LoadRealtimeAsync(),
                    LoadTopProductsAsync()
                );
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadOverviewAsync()
        {
            var result = await _apiService.GetAnalyticsOverviewAsync();
            var json = JsonSerializer.Serialize(result);
            var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var today = doc.RootElement.GetProperty("today");
            TodayPageviews       = today.GetProperty("pageviews").GetInt32();
            TodaySessions        = today.GetProperty("unique_sessions").GetInt32();
            TodayUniqueVisitors  = today.GetProperty("unique_visitors").GetInt32();

            var week = doc.RootElement.GetProperty("this_week");
            WeekPageviews       = week.GetProperty("pageviews").GetInt32();
            WeekNewVisitors     = week.GetProperty("new_visitors").GetInt32();
            WeekUniqueVisitors  = week.GetProperty("unique_visitors").GetInt32();

            var month = doc.RootElement.GetProperty("this_month");
            MonthPageviews       = month.GetProperty("pageviews").GetInt32();
            MonthUniqueVisitors  = month.GetProperty("unique_visitors").GetInt32();
        }

        private async Task LoadRealtimeAsync()
        {
            var result = await _apiService.GetAnalyticsRealtimeAsync();
            var json = JsonSerializer.Serialize(result);
            var doc = JsonDocument.Parse(json);

            ActiveSessions = doc.RootElement.GetProperty("active_sessions").GetInt32();
        }

        /// <summary>
        /// Lightweight refresh for the "aktív most" counter only.
        /// Called on a 30s timer from the page so the live count stays fresh
        /// without reloading the full overview + top products.
        /// </summary>
        public async Task RefreshActiveSessionsAsync()
        {
            try
            {
                await LoadRealtimeAsync();
            }
            catch
            {
                // Silent — a dropped heartbeat shouldn't show an error banner.
            }
        }

        private async Task LoadTopProductsAsync()
        {
            var result = await _apiService.GetAnalyticsTopProductsAsync();
            var json = JsonSerializer.Serialize(result);
            // Laravel a decimal oszlopokat ("price", "subtotal") stringként küldi a
            // `decimal:2` cast miatt — ezért kell az AllowReadingFromString, különben
            // a TotalRevenue / Product.Price deszerializálás eldobna.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            var items = JsonSerializer.Deserialize<List<TopProduct>>(json, options);

            TopProducts.Clear();
            if (items != null)
                foreach (var item in items)
                    TopProducts.Add(item);
        }
    }
}