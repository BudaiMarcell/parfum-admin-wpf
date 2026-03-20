using System.Windows.Controls;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class ProductsPage : Page
    {
        private readonly ProductsViewModel _viewModel;

        public ProductsPage(ProductsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.LoadProductsAsync();
        }
    }
}