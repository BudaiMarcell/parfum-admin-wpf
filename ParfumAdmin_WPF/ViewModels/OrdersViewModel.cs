using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ParfumAdmin_WPF.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private CancellationTokenSource _debounceCts;
        private bool _suppressReload;

        public ObservableCollection<Order> Orders { get; } = new();

        // --- Szűrők ---
        public List<string> StatusOptions { get; } = new()
        {
            "Összes", "pending", "processing", "shipped", "arrived", "canceled", "refunded"
        };

        private string _selectedStatus = "Összes";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { if (SetProperty(ref _selectedStatus, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value) && !_suppressReload) _ = ReloadDebouncedAsync(); }
        }

        // --- Kiválasztott rendelés ---
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        // --- Lapozás ---
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

        // --- Parancsok ---
        public ICommand LoadOrdersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand UpdatePaymentCommand { get; }

        public Func<Order, bool> ConfirmDelete { get; set; }

        public OrdersViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadOrdersCommand    = new RelayCommand(async _ => await LoadOrdersAsync());
            ResetFiltersCommand  = new RelayCommand(async _ => await ResetFiltersAsync());
            NextPageCommand      = new RelayCommand(async _ => await NextPageAsync(),     _ => CurrentPage < TotalPages);
            PreviousPageCommand  = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            DeleteOrderCommand   = new RelayCommand(async _ => await DeleteOrderAsync(),  _ => SelectedOrder != null);
            UpdateStatusCommand  = new RelayCommand(
                async param => await UpdateStatusAsync(param as string),
                _ => SelectedOrder != null);
            UpdatePaymentCommand = new RelayCommand(
                async param => await UpdatePaymentAsync(param as string),
                _ => SelectedOrder != null);
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var status = SelectedStatus == "Összes" ? null : SelectedStatus;
                var search = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery;

                var result = await _apiService.GetOrdersAsync(CurrentPage, status, search);

                Orders.Clear();
                foreach (var order in result.Data)
                    Orders.Add(order);

                TotalPages = result.LastPage;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba a rendelések betöltésekor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReloadAsync()
        {
            CurrentPage = 1;
            await LoadOrdersAsync();
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
            SearchQuery    = null;
            SelectedStatus = "Összes";
            _suppressReload = false;
            await ReloadAsync();
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadOrdersAsync();
        }

        private async Task PreviousPageAsync()
        {
            CurrentPage--;
            await LoadOrdersAsync();
        }

        private async Task DeleteOrderAsync()
        {
            if (SelectedOrder == null) return;
            if (ConfirmDelete != null && !ConfirmDelete(SelectedOrder)) return;
            try
            {
                IsLoading = true;
                await _apiService.DeleteOrderAsync(SelectedOrder.Id);
                Orders.Remove(SelectedOrder);
                SelectedOrder = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba törléskor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateStatusAsync(string newStatus)
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(newStatus)) return;
            try
            {
                IsLoading = true;
                await _apiService.UpdateOrderStatusAsync(SelectedOrder.Id, newStatus);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Státusz módosítás sikertelen: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdatePaymentAsync(string newPaymentStatus)
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(newPaymentStatus)) return;
            try
            {
                IsLoading = true;
                await _apiService.UpdateOrderPaymentAsync(SelectedOrder.Id, newPaymentStatus);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Fizetési státusz módosítás sikertelen: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
