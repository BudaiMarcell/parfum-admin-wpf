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
    public class CouponsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private CancellationTokenSource _debounceCts;
        private bool _suppressReload;

        public ObservableCollection<Coupon> Coupons { get; } = new();

        // Lapozás
        private int _currentPage = 1;
        public int CurrentPage { get => _currentPage; set => SetProperty(ref _currentPage, value); }

        private int _totalPages = 1;
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }

        // Szűrők — minden változáskor újratölt (search text debounce-olva)
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value) && !_suppressReload) _ = ReloadDebouncedAsync(); }
        }

        private string _selectedDiscountType;
        public string SelectedDiscountType
        {
            get => _selectedDiscountType;
            set { if (SetProperty(ref _selectedDiscountType, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { if (SetProperty(ref _selectedStatus, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private Coupon _selectedCoupon;
        public Coupon SelectedCoupon { get => _selectedCoupon; set => SetProperty(ref _selectedCoupon, value); }

        public List<string> DiscountTypeOptions { get; } = new() { "Összes", "percentage", "fixed" };
        public List<string> StatusOptions { get; } = new() { "Összes", "active", "expired" };

        public ICommand LoadCouponsCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand AddCouponCommand { get; }
        public ICommand EditCouponCommand { get; }
        public ICommand DeleteCouponCommand { get; }

        public event Action OnAddCouponRequested;
        public event Action<Coupon> OnEditCouponRequested;

        // A View állítja be — igaz esetén mehet a törlés.
        public Func<Coupon, bool> ConfirmDelete { get; set; }

        public CouponsViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadCouponsCommand  = new RelayCommand(async _ => await LoadCouponsAsync());
            ResetFiltersCommand = new RelayCommand(async _ => await ResetFiltersAsync());
            NextPageCommand     = new RelayCommand(async _ => await NextPageAsync(),     _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            AddCouponCommand    = new RelayCommand(_ => OnAddCouponRequested?.Invoke());
            EditCouponCommand   = new RelayCommand(
                _ => { if (SelectedCoupon != null) OnEditCouponRequested?.Invoke(SelectedCoupon); },
                _ => SelectedCoupon != null);
            DeleteCouponCommand = new RelayCommand(async _ => await DeleteCouponAsync(), _ => SelectedCoupon != null);

            _selectedDiscountType = "Összes";
            _selectedStatus       = "Összes";
        }

        public async Task LoadCouponsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var search = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery;
                var type   = SelectedDiscountType == "Összes" || string.IsNullOrEmpty(SelectedDiscountType) ? null : SelectedDiscountType;
                var status = SelectedStatus == "Összes" || string.IsNullOrEmpty(SelectedStatus) ? null : SelectedStatus;

                var result = await _apiService.GetCouponsAsync(CurrentPage, search, type, status);

                Coupons.Clear();
                foreach (var c in result.Data)
                    Coupons.Add(c);

                TotalPages = result.LastPage;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba a kuponok betöltésekor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReloadAsync()
        {
            CurrentPage = 1;
            await LoadCouponsAsync();
        }

        // Gépelés közben ne terheljük az API-t — 350 ms után tölt.
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
            SearchQuery = null;
            SelectedDiscountType = "Összes";
            SelectedStatus = "Összes";
            _suppressReload = false;
            await ReloadAsync();
        }

        private async Task NextPageAsync()     { CurrentPage++; await LoadCouponsAsync(); }
        private async Task PreviousPageAsync() { CurrentPage--; await LoadCouponsAsync(); }

        private async Task DeleteCouponAsync()
        {
            if (SelectedCoupon == null) return;
            if (ConfirmDelete != null && !ConfirmDelete(SelectedCoupon)) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteCouponAsync(SelectedCoupon.Id);
                Coupons.Remove(SelectedCoupon);
                SelectedCoupon = null;
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
    }
}
