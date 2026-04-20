using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;

namespace ParfumAdmin_WPF.ViewModels
{
    public class CouponFormViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        private int? _editingCouponId;

        public bool IsEditMode => _editingCouponId.HasValue;
        public string WindowTitle => IsEditMode ? "Kupon szerkesztése" : "Új kupon hozzáadása";
        public string Subtitle => IsEditMode
            ? "Módosítsd a kupon adatait, majd kattints a Mentés gombra."
            : "Add meg a kupon adatait. Ha üresen hagyod a kódot, generálunk egyet.";

        public string[] DiscountTypeOptions { get; } = { "percentage", "fixed" };

        private string _couponCode = string.Empty;
        public string CouponCode { get => _couponCode; set => SetProperty(ref _couponCode, value); }

        private string _selectedDiscountType = "percentage";
        public string SelectedDiscountType { get => _selectedDiscountType; set => SetProperty(ref _selectedDiscountType, value); }

        private decimal _discountValue;
        public decimal DiscountValue { get => _discountValue; set => SetProperty(ref _discountValue, value); }

        private DateTime _expiryDate = DateTime.Today.AddMonths(1);
        public DateTime ExpiryDate { get => _expiryDate; set => SetProperty(ref _expiryDate, value); }

        private int? _usageLimit;
        public int? UsageLimit { get => _usageLimit; set => SetProperty(ref _usageLimit, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> OnRequestClose;

        public CouponFormViewModel(IApiService apiService)
        {
            _apiService = apiService;
            SaveCommand   = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnRequestClose?.Invoke(false));
        }

        public void InitializeForAdd()
        {
            _editingCouponId = null;
            CouponCode = string.Empty;
            SelectedDiscountType = "percentage";
            DiscountValue = 10;
            ExpiryDate = DateTime.Today.AddMonths(1);
            UsageLimit = null;
            IsActive = true;
            ErrorMessage = null;
            NotifyMode();
        }

        public void InitializeForEdit(Coupon c)
        {
            _editingCouponId = c.Id;
            CouponCode = c.CouponCode ?? string.Empty;
            SelectedDiscountType = c.DiscountType ?? "percentage";
            DiscountValue = c.DiscountValue;
            ExpiryDate = c.ExpiryDate == default ? DateTime.Today.AddMonths(1) : c.ExpiryDate;
            UsageLimit = c.UsageLimit;
            IsActive = c.IsActive;
            ErrorMessage = null;
            NotifyMode();
        }

        private void NotifyMode()
        {
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Subtitle));
        }

        private async Task SaveAsync()
        {
            ErrorMessage = null;

            if (DiscountValue < 0)
            {
                ErrorMessage = "A kedvezmény nem lehet negatív.";
                return;
            }
            if (SelectedDiscountType == "percentage" && DiscountValue > 100)
            {
                ErrorMessage = "A százalékos kedvezmény nem lehet nagyobb 100-nál.";
                return;
            }
            if (ExpiryDate < DateTime.Today && !IsEditMode)
            {
                ErrorMessage = "A lejárati dátum nem lehet múltbeli.";
                return;
            }
            if (UsageLimit.HasValue && UsageLimit.Value < 1)
            {
                ErrorMessage = "A felhasználási limit legalább 1 legyen (vagy hagyd üresen).";
                return;
            }

            try
            {
                IsLoading = true;

                var payload = new
                {
                    coupon_code    = string.IsNullOrWhiteSpace(CouponCode) ? null : CouponCode.Trim().ToUpper(),
                    discount_type  = SelectedDiscountType,
                    discount_value = DiscountValue,
                    expiry_date    = ExpiryDate.ToString("yyyy-MM-dd"),
                    usage_limit    = UsageLimit,
                    is_active      = IsActive,
                };

                if (IsEditMode)
                    await _apiService.UpdateCouponAsync(_editingCouponId!.Value, payload);
                else
                    await _apiService.CreateCouponAsync(payload);

                OnRequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Mentés sikertelen: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
