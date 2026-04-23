using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;

namespace ParfumAdmin_WPF.ViewModels
{
    public class AuditLogsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private CancellationTokenSource _debounceCts;
        private bool _suppressReload;

        public ObservableCollection<AuditLog> Logs { get; } = new();

        public List<string> ActionOptions { get; } = new()
        {
            "Összes", "created", "updated", "deleted",
            "bulk_deleted", "bulk_updated",
            "status_changed", "payment_changed"
        };

        public List<string> ModelTypeOptions { get; } = new()
        {
            "Összes", "Product", "Order", "Coupon"
        };

        private string _selectedAction = "Összes";
        public string SelectedAction
        {
            get => _selectedAction;
            set { if (SetProperty(ref _selectedAction, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private string _selectedModelType = "Összes";
        public string SelectedModelType
        {
            get => _selectedModelType;
            set { if (SetProperty(ref _selectedModelType, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value) && !_suppressReload) _ = ReloadDebouncedAsync(); }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public ICommand LoadLogsCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        public AuditLogsViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadLogsCommand     = new RelayCommand(async _ => await LoadLogsAsync());
            ResetFiltersCommand = new RelayCommand(async _ => await ResetFiltersAsync());
            NextPageCommand     = new RelayCommand(async _ => await NextPageAsync(),     _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            RefreshCommand      = new RelayCommand(async _ => await LoadLogsAsync());
        }

        public async Task LoadLogsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var action = SelectedAction == "Összes" ? null : SelectedAction;
                var model  = SelectedModelType == "Összes" ? null : SelectedModelType;
                var search = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery;

                var result = await _apiService.GetAuditLogsAsync(CurrentPage, action, model, search);

                Logs.Clear();
                foreach (var l in result.Data)
                    Logs.Add(l);

                TotalPages = result.LastPage;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba a naplók betöltésekor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReloadAsync()
        {
            CurrentPage = 1;
            await LoadLogsAsync();
        }

        private async Task ReloadDebouncedAsync()
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;
            try
            {
                await Task.Delay(350, token);
                if (token.IsCancellationRequested) return;
                await ReloadAsync();
            }
            catch (TaskCanceledException) { }
        }

        private async Task ResetFiltersAsync()
        {
            _suppressReload = true;
            SearchQuery        = null;
            SelectedAction     = "Összes";
            SelectedModelType  = "Összes";
            _suppressReload = false;
            await ReloadAsync();
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadLogsAsync();
        }

        private async Task PreviousPageAsync()
        {
            CurrentPage--;
            await LoadLogsAsync();
        }
    }
}
