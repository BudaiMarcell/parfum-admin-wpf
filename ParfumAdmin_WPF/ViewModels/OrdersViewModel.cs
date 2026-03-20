using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace ParfumAdmin_WPF.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        public ObservableCollection<Order> Orders { get; } = new();

        public List<string> StatusOptions { get; } = new()
        {
            "all", "pending", "processing", "shipped", "arrived", "canceled", "refunded"
        };

        private string _selectedStatus = "all";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                _ = LoadOrdersAsync();
            }
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public OrdersViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadOrdersCommand = new RelayCommand(async _ => await LoadOrdersAsync());
            UpdateStatusCommand = new RelayCommand(async param => await UpdateStatusAsync(param as string), _ => SelectedOrder != null);
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var status = SelectedStatus == "all" ? null : SelectedStatus;
                var result = await _apiService.GetOrdersAsync(CurrentPage, status);

                Orders.Clear();
                foreach (var order in result.Data)
                    Orders.Add(order);

                TotalPages = result.LastPage;
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

        private async Task UpdateStatusAsync(string newStatus)
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(newStatus)) return;

            try
            {
                IsLoading = true;
                var updated = await _apiService.UpdateOrderStatusAsync(SelectedOrder.Id, newStatus);
                SelectedOrder.Status = updated.Status;
                await LoadOrdersAsync();
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
    }
}
