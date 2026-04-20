using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;

namespace ParfumAdmin_WPF.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private CancellationTokenSource _debounceCts;
        private bool _suppressReload;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Product> SelectedProducts { get; } = new();

        public int SelectedCount => SelectedProducts.Count;
        public bool HasMultipleSelected => SelectedProducts.Count > 1;

        // Lapozás
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

        // Szűrők
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value) && !_suppressReload) _ = ReloadDebouncedAsync(); }
        }

        private string _selectedGender;
        public string SelectedGender
        {
            get => _selectedGender;
            set { if (SetProperty(ref _selectedGender, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { if (SetProperty(ref _selectedCategory, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        private bool _showLowStock;
        public bool ShowLowStock
        {
            get => _showLowStock;
            set { if (SetProperty(ref _showLowStock, value) && !_suppressReload) _ = ReloadAsync(); }
        }

        // Kiválasztott termék
        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public List<string> GenderOptions { get; } = new()
        {
            "Összes", "male", "female", "unisex"
        };

        public ICommand LoadProductsCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand BulkDeleteCommand { get; }
        public ICommand BulkActivateCommand { get; }
        public ICommand BulkDeactivateCommand { get; }

        // Események az űrlap ablak megnyitásához (Add / Edit)
        public event Action OnAddProductRequested;
        public event Action<Product> OnEditProductRequested;

        public Func<Product, bool> ConfirmDelete { get; set; }
        public Func<int, bool> ConfirmBulkDelete { get; set; }

        public ProductsViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            ResetFiltersCommand = new RelayCommand(async _ => await ResetFiltersAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);
            AddProductCommand = new RelayCommand(_ => OnAddProductRequested?.Invoke());
            EditProductCommand = new RelayCommand(
                _ => { if (SelectedProduct != null) OnEditProductRequested?.Invoke(SelectedProduct); },
                _ => SelectedProduct != null && SelectedProducts.Count <= 1);

            BulkDeleteCommand = new RelayCommand(async _ => await BulkDeleteAsync(), _ => SelectedProducts.Count > 0);
            BulkActivateCommand = new RelayCommand(async _ => await BulkSetActiveAsync(true), _ => SelectedProducts.Count > 0);
            BulkDeactivateCommand = new RelayCommand(async _ => await BulkSetActiveAsync(false), _ => SelectedProducts.Count > 0);

            SelectedProducts.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(HasMultipleSelected));
                CommandManager.InvalidateRequerySuggested();
            };

            _selectedGender = "Összes";
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // kategóriák betöltése ha még üres
                if (Categories.Count == 0)
                    await LoadCategoriesAsync();

                var search = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery;
                var gender = SelectedGender == "Összes" || string.IsNullOrEmpty(SelectedGender) ? null : SelectedGender;
                var categoryId = SelectedCategory?.Id;

                var result = await _apiService.GetProductsAsync(CurrentPage, search, gender, categoryId, ShowLowStock);

                Products.Clear();
                foreach (var product in result.Data)
                    Products.Add(product);

                TotalPages = result.LastPage;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba a termékek betöltésekor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var cats = await _apiService.GetAdminCategoriesAsync();
            _suppressReload = true;
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = "Összes kategória" });
            foreach (var c in cats)
                Categories.Add(c);
            SelectedCategory = Categories[0];
            _suppressReload = false;
        }

        private async Task ReloadAsync()
        {
            CurrentPage = 1;
            await LoadProductsAsync();
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
            SearchQuery = null;
            SelectedGender = "Összes";
            SelectedCategory = Categories.Count > 0 ? Categories[0] : null;
            ShowLowStock = false;
            _suppressReload = false;
            await ReloadAsync();
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadProductsAsync();
        }

        private async Task PreviousPageAsync()
        {
            CurrentPage--;
            await LoadProductsAsync();
        }

        public async Task ToggleActiveAsync(Product product)
        {
            if (product == null) return;

            try
            {
                await _apiService.UpdateProductAsync(product.Id, new { is_active = product.IsActive });
            }
            catch (Exception ex)
            {
                product.IsActive = !product.IsActive;
                ErrorMessage = "Nem sikerült menteni az aktív állapotot: " + ex.Message;
                await LoadProductsAsync();
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;
            if (ConfirmDelete != null && !ConfirmDelete(SelectedProduct)) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteProductAsync(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
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

        private async Task BulkDeleteAsync()
        {
            if (SelectedProducts.Count == 0) return;
            if (ConfirmBulkDelete != null && !ConfirmBulkDelete(SelectedProducts.Count)) return;

            try
            {
                IsLoading = true;
                var ids = SelectedProducts.Select(p => p.Id).ToList();
                await _apiService.BulkDeleteProductsAsync(ids);
                foreach (var p in SelectedProducts.ToList())
                    Products.Remove(p);
                SelectedProducts.Clear();
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba tömeges törléskor: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BulkSetActiveAsync(bool isActive)
        {
            if (SelectedProducts.Count == 0) return;

            try
            {
                IsLoading = true;
                var ids = SelectedProducts.Select(p => p.Id).ToList();
                await _apiService.BulkUpdateProductsAsync(ids, new { is_active = isActive });
                foreach (var p in SelectedProducts)
                    p.IsActive = isActive;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Hiba tömeges frissítéskor: " + ex.Message;
                await LoadProductsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
