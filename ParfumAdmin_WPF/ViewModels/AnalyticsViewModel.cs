using System.Collections.ObjectModel;
using System.Windows.Input;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;

namespace ParfumAdmin_WPF.ViewModels
{
    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        // Overview snapshot
        private AnalyticsOverview _overview;
        public AnalyticsOverview Overview
        {
            get => _overview;
            set => SetProperty(ref _overview, value);
        }

        // Data for charts — raw numbers/labels for the code-behind to render.
        public List<string> DailyLabels  { get; private set; } = new();
        public List<double> DailyValues  { get; private set; } = new();   // pageviews per day
        public List<double> SessionValues { get; private set; } = new();  // sessions per day
        public List<double> OrderValues   { get; private set; } = new();  // orders per day
        public List<double> RevenueValues { get; private set; } = new();  // revenue per day

        public List<string> HourlyLabels { get; private set; } = new();
        public List<double> HourlyValues { get; private set; } = new();

        public DeviceStats Devices { get; private set; }
        public FunnelStats Funnel  { get; private set; }

        // Which metric is shown in the big line chart
        public List<string> MetricOptions { get; } = new() { "Oldalmegtekintések", "Sessionök", "Rendelések", "Bevétel (Ft)" };

        private string _selectedMetric = "Oldalmegtekintések";
        public string SelectedMetric
        {
            get => _selectedMetric;
            set
            {
                if (SetProperty(ref _selectedMetric, value))
                    MetricChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        // Days window for the daily chart
        public List<int> DaysOptions { get; } = new() { 7, 14, 30, 60, 90 };

        private int _selectedDays = 30;
        public int SelectedDays
        {
            get => _selectedDays;
            set
            {
                if (SetProperty(ref _selectedDays, value))
                    _ = LoadDailyAsync();
            }
        }

        public event System.EventHandler DataLoaded;
        public event System.EventHandler MetricChanged;

        public ICommand RefreshCommand { get; }

        public AnalyticsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new RelayCommand(async _ => await LoadAllAsync());
        }

        public async Task LoadAllAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // Mind az 5 végpont független — párhuzamosan futtatjuk, hogy a teljes
                // betöltés a leglassabb kérés idejébe kerüljön.
                var overviewTask = _apiService.GetAnalyticsOverviewTypedAsync();
                var dailyTask    = _apiService.GetAnalyticsDailyAsync(SelectedDays);
                var hourlyTask   = _apiService.GetAnalyticsHourlyTypedAsync();
                var devicesTask  = _apiService.GetAnalyticsDevicesAsync();
                var funnelTask   = _apiService.GetAnalyticsFunnelAsync();

                await System.Threading.Tasks.Task.WhenAll(
                    overviewTask, dailyTask, hourlyTask, devicesTask, funnelTask);

                Overview = overviewTask.Result;
                OnPropertyChanged(nameof(Overview));

                var pts = dailyTask.Result?.Series ?? new List<DailyPoint>();
                DailyLabels   = pts.ConvertAll(p => ShortDate(p.Date));
                DailyValues   = pts.ConvertAll(p => (double)p.Pageviews);
                SessionValues = pts.ConvertAll(p => (double)p.Sessions);
                OrderValues   = pts.ConvertAll(p => (double)p.Orders);
                RevenueValues = pts.ConvertAll(p => p.Revenue);

                var hourly = hourlyTask.Result;
                HourlyLabels = (hourly?.Labels ?? new List<int>()).ConvertAll(i => i.ToString("00") + "h");
                HourlyValues = (hourly?.Data   ?? new List<int>()).ConvertAll(i => (double)i);

                Devices = devicesTask.Result ?? new DeviceStats();
                Funnel  = funnelTask.Result  ?? new FunnelStats();

                DataLoaded?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDailyAsync(bool suppressEvent = false)
        {
            var daily = await _apiService.GetAnalyticsDailyAsync(SelectedDays);
            var pts   = daily?.Series ?? new List<DailyPoint>();

            DailyLabels   = pts.ConvertAll(p => ShortDate(p.Date));
            DailyValues   = pts.ConvertAll(p => (double)p.Pageviews);
            SessionValues = pts.ConvertAll(p => (double)p.Sessions);
            OrderValues   = pts.ConvertAll(p => (double)p.Orders);
            RevenueValues = pts.ConvertAll(p => p.Revenue);

            if (!suppressEvent) DataLoaded?.Invoke(this, System.EventArgs.Empty);
        }

        private static string ShortDate(string iso)
        {
            // "2026-04-22" → "04-22"
            if (string.IsNullOrEmpty(iso) || iso.Length < 10) return iso ?? "";
            return iso.Substring(5);
        }

        public List<double> GetSelectedSeries() => SelectedMetric switch
        {
            "Oldalmegtekintések" => DailyValues,
            "Sessionök"          => SessionValues,
            "Rendelések"         => OrderValues,
            "Bevétel (Ft)"       => RevenueValues,
            _                    => DailyValues,
        };
    }
}
