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

        private int _monthPageviews;
        public int MonthPageviews
        {
            get => _monthPageviews;
            set => SetProperty(ref _monthPageviews, value);
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

                await LoadOverviewAsync();
                await LoadRealtimeAsync();
                await LoadTopProductsAsync();
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
            TodayPageviews = today.GetProperty("pageviews").GetInt32();
            TodaySessions = today.GetProperty("unique_sessions").GetInt32();

            var week = doc.RootElement.GetProperty("this_week");
            WeekPageviews = week.GetProperty("pageviews").GetInt32();
            WeekNewVisitors = week.GetProperty("new_visitors").GetInt32();

            var month = doc.RootElement.GetProperty("this_month");
            MonthPageviews = month.GetProperty("pageviews").GetInt32();
        }

        private async Task LoadRealtimeAsync()
        {
            var result = await _apiService.GetAnalyticsRealtimeAsync();
            var json = JsonSerializer.Serialize(result);
            var doc = JsonDocument.Parse(json);

            ActiveSessions = doc.RootElement.GetProperty("active_sessions").GetInt32();
        }

        private async Task LoadTopProductsAsync()
        {
            var result = await _apiService.GetAnalyticsTopProductsAsync();
            var json = JsonSerializer.Serialize(result);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<List<TopProduct>>(json, options);

            TopProducts.Clear();
            if (items != null)
                foreach (var item in items)
                    TopProducts.Add(item);
        }
    }
}