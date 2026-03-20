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
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        public ObservableCollection<Product> Products { get; } = new();

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

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductsViewModel(IApiService apiService)
        {
            _apiService = apiService;

            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var result = await _apiService.GetProductsAsync(CurrentPage, SearchQuery);

                Products.Clear();
                foreach (var product in result.Data)
                    Products.Add(product);

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

        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadProductsAsync();
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

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteProductAsync(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
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
    }
}
