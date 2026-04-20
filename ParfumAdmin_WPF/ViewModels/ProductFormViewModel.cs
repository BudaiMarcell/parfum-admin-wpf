using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;

namespace ParfumAdmin_WPF.ViewModels
{
    /// <summary>
    /// Közös VM a termék létrehozás és szerkesztés űrlaphoz.
    /// InitializeForAdd() vagy InitializeForEdit(product) hívásával állítjuk be a módot
    /// megnyitás előtt, utána LoadCategoriesAsync() tölti be a kategóriákat és -- ha edit módban
    /// vagyunk -- kiválasztja a termék aktuális kategóriáját.
    /// </summary>
    public class ProductFormViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        private int? _editingProductId;
        // Az edit módban a SelectedCategory-t csak a kategórialista betöltése UTÁN tudjuk beállítani.
        // Addig itt tároljuk a szerkesztendő termék kategória-id-jét.
        private int? _pendingCategoryId;

        public bool IsEditMode => _editingProductId.HasValue;
        public string WindowTitle => IsEditMode ? "Termék szerkesztése" : "Új termék hozzáadása";
        public string Subtitle => IsEditMode
            ? "Módosítsd a termék adatait, majd kattints a Mentés gombra."
            : "Add meg a termék adatait, majd kattints a Mentés gombra.";

        public ObservableCollection<Category> Categories { get; } = new();
        public string[] GenderOptions { get; } = { "male", "female", "unisex" };

        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _description = string.Empty;
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        private decimal _price;
        public decimal Price { get => _price; set => SetProperty(ref _price, value); }

        private int _stockQuantity;
        public int StockQuantity { get => _stockQuantity; set => SetProperty(ref _stockQuantity, value); }

        private int? _volumeMl;
        public int? VolumeMl { get => _volumeMl; set => SetProperty(ref _volumeMl, value); }

        private string _selectedGender = "unisex";
        public string SelectedGender { get => _selectedGender; set => SetProperty(ref _selectedGender, value); }

        private Category? _selectedCategory;
        public Category? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? OnRequestClose;

        public ProductFormViewModel(IApiService apiService)
        {
            _apiService = apiService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnRequestClose?.Invoke(false));
        }

        /// <summary>Új termék felvitelre állítja be az űrlapot.</summary>
        public void InitializeForAdd()
        {
            _editingProductId = null;
            _pendingCategoryId = null;
            Name = string.Empty;
            Description = string.Empty;
            Price = 0;
            StockQuantity = 0;
            VolumeMl = null;
            SelectedGender = "unisex";
            IsActive = true;
            ErrorMessage = null;
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Subtitle));
        }

        /// <summary>Meglévő termék szerkesztésére állítja be az űrlapot.</summary>
        public void InitializeForEdit(Product product)
        {
            _editingProductId = product.Id;
            _pendingCategoryId = product.CategoryId;
            Name = product.Name ?? string.Empty;
            Description = product.Description ?? string.Empty;
            Price = product.Price;
            StockQuantity = product.StockQuantity;
            VolumeMl = product.VolumeMl;
            SelectedGender = string.IsNullOrEmpty(product.Gender) ? "unisex" : product.Gender;
            IsActive = product.IsActive;
            ErrorMessage = null;
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Subtitle));
        }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _apiService.GetAdminCategoriesAsync();
                Categories.Clear();
                foreach (var c in cats)
                    Categories.Add(c);

                // Edit módban a szerkesztendő termék kategóriáját állítjuk be,
                // add módban az elsőt.
                if (_pendingCategoryId.HasValue)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == _pendingCategoryId.Value);
                }
                else if (Categories.Count > 0 && SelectedCategory == null)
                {
                    SelectedCategory = Categories[0];
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Nem sikerült betölteni a kategóriákat: " + ex.Message;
            }
        }

        private async Task SaveAsync()
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "A név megadása kötelező.";
                return;
            }
            if (SelectedCategory == null)
            {
                ErrorMessage = "Válassz kategóriát.";
                return;
            }
            if (Price < 0)
            {
                ErrorMessage = "Az ár nem lehet negatív.";
                return;
            }
            if (StockQuantity < 0)
            {
                ErrorMessage = "A készlet nem lehet negatív.";
                return;
            }

            try
            {
                IsLoading = true;

                var payload = new
                {
                    category_id = SelectedCategory.Id,
                    name = Name.Trim(),
                    description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    price = Price,
                    stock_quantity = StockQuantity,
                    volume_ml = VolumeMl,
                    gender = SelectedGender,
                    is_active = IsActive
                };

                if (IsEditMode)
                {
                    await _apiService.UpdateProductAsync(_editingProductId!.Value, payload);
                }
                else
                {
                    await _apiService.CreateProductAsync(payload);
                }

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
