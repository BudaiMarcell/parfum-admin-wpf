using System.Windows.Controls;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }
    }
}