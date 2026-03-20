using System.Windows.Controls;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class OrdersPage : Page
    {
        private readonly OrdersViewModel _viewModel;

        public OrdersPage(OrdersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.LoadOrdersAsync();
        }
    }
}